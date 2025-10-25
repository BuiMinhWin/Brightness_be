using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Application.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IChatSessionRepository _sessionRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IConversationSummaryRepository _summaryRepository;
        private readonly IGeminiService _geminiService;
        private readonly IUserContextService _userContextService;
        private readonly IMessageLimitService _messageLimitService;
        private readonly IIntentDetectionService _intentDetectionService;
        private readonly IInputSanitizationService _inputSanitizationService;
        private readonly IResponseValidationService _responseValidationService;
        private readonly IUserContextCacheService _contextCacheService;
        private readonly IPersonalCoachApiService _personalCoachApiService;
        private readonly IActionExecutorService _actionExecutorService;
        private readonly ILogger<ConversationService> _logger;
        private readonly int _summarizationThreshold;

        public ConversationService(
            IChatSessionRepository sessionRepository,
            IChatMessageRepository messageRepository,
            IConversationSummaryRepository summaryRepository,
            IGeminiService geminiService,
            IUserContextService userContextService,
            IMessageLimitService messageLimitService,
            IIntentDetectionService intentDetectionService,
            IInputSanitizationService inputSanitizationService,
            IResponseValidationService responseValidationService,
            IUserContextCacheService contextCacheService,
            IPersonalCoachApiService personalCoachApiService,
            IActionExecutorService actionExecutorService,
            ILogger<ConversationService> logger,
            IConfiguration configuration)
        {
            _sessionRepository = sessionRepository;
            _messageRepository = messageRepository;
            _summaryRepository = summaryRepository;
            _geminiService = geminiService;
            _userContextService = userContextService;
            _messageLimitService = messageLimitService;
            _intentDetectionService = intentDetectionService;
            _inputSanitizationService = inputSanitizationService;
            _responseValidationService = responseValidationService;
            _contextCacheService = contextCacheService;
            _personalCoachApiService = personalCoachApiService;
            _actionExecutorService = actionExecutorService;
            _logger = logger;
            
            _summarizationThreshold = int.Parse(
                configuration["Conversation:SummarizationThreshold"] 
                ?? configuration["CONVERSATION_SUMMARIZATION_THRESHOLD"] 
                ?? "10");
        }

        public async Task<SendMessageResponse> SendMessageAsync(Guid userId, SendMessageRequest request)
        {
            // Check message limits
            if (!await _messageLimitService.CanSendMessageAsync(userId))
            {
                throw new InvalidOperationException("Message limit reached. Please upgrade your subscription.");
            }

            // Sanitize user input
            var sanitizedMessage = _inputSanitizationService.SanitizeInput(request.Message);
            
            if (string.IsNullOrWhiteSpace(sanitizedMessage))
            {
                throw new ArgumentException("Message cannot be empty after sanitization");
            }

            // Get or create session
            Guid sessionId;
            if (request.SessionId.HasValue)
            {
                var existingSession = await _sessionRepository.GetByIdAsync(request.SessionId.Value);
                if (existingSession == null || existingSession.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Session does not belong to user");
                }
                sessionId = request.SessionId.Value;
            }
            else
            {
                var newSession = await CreateSessionAsync(userId, "New Chat");
                sessionId = newSession.Id;
            }

            // Build user context first (needed for action execution and AI responses)
            var userContext = await GetOrBuildUserContextAsync(userId, sessionId);
            
            // Check if user is confirming or canceling a pending action
            var isConfirmation = IsConfirmationResponse(sanitizedMessage);
            var isCancellation = IsCancellationResponse(sanitizedMessage);
            
            if (isConfirmation || isCancellation)
            {
                var pendingAction = await GetPendingActionAsync(sessionId);
                
                if (pendingAction != null)
                {
                    if (isConfirmation)
                    {
                        // User confirmed - execute the pending action
                        _logger.LogInformation("Executing confirmed action {ActionType} for user {UserId}", 
                            pendingAction.Type, userId);
                        
                        var confirmedActionResult = await _actionExecutorService.ExecuteActionAsync(
                            userId, pendingAction, userContext);
                        
                        // Clear pending action
                        await ClearPendingActionAsync(sessionId);
                        
                        // Save user message
                        await SaveUserMessageAsync(sessionId, userId, sanitizedMessage);
                        
                        // Generate and save AI response based on action result
                        var confirmedResponse = await GenerateActionResponseAsync(confirmedActionResult, userContext);
                        var aiMessage = await SaveAiMessageAsync(sessionId, userId, confirmedResponse);
                        
                        await CheckAndSummarizeIfNeededAsync(sessionId);
                        await _messageLimitService.IncrementMessageCountAsync(userId);
                        
                        var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
                        var creditBalance = await _personalCoachApiService.GetCreditBalanceAsync(userId);
                        
                        return new SendMessageResponse(
                            SessionId: sessionId,
                            Message: confirmedResponse,
                            Role: "ai",
                            RemainingMessages: limits.RemainingDaily,
                            DailyLimit: limits.DailyLimit,
                            CreatedAt: aiMessage.CreatedAt,
                            RemainingCredits: creditBalance?.RemainingCredits,
                            MonthlyCredits: creditBalance?.MonthlyCredits,
                            SubscriptionTier: creditBalance?.SubscriptionTier,
                            ActionResult: confirmedActionResult
                        );
                    }
                    else
                    {
                        // User canceled
                        await ClearPendingActionAsync(sessionId);
                        await SaveUserMessageAsync(sessionId, userId, sanitizedMessage);
                        
                        var cancelResponse = "No problem! I've canceled that action. What else can I help you with?";
                        var aiMessage = await SaveAiMessageAsync(sessionId, userId, cancelResponse);
                        
                        await _messageLimitService.IncrementMessageCountAsync(userId);
                        var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
                        var creditBalance = await _personalCoachApiService.GetCreditBalanceAsync(userId);
                        
                        return new SendMessageResponse(
                            SessionId: sessionId,
                            Message: cancelResponse,
                            Role: "ai",
                            RemainingMessages: limits.RemainingDaily,
                            DailyLimit: limits.DailyLimit,
                            CreatedAt: aiMessage.CreatedAt,
                            RemainingCredits: creditBalance?.RemainingCredits,
                            MonthlyCredits: creditBalance?.MonthlyCredits,
                            SubscriptionTier: creditBalance?.SubscriptionTier
                        );
                    }
                }
            }
            
            // Run intent detection
            var intent = await _intentDetectionService.DetectIntentAsync(sanitizedMessage);
            
            // If unsafe intent detected, reject
            if (!intent.IsSafe)
            {
                _logger.LogWarning("Unsafe intent detected for user {UserId}: {Message}", 
                    userId, sanitizedMessage.Substring(0, Math.Min(100, sanitizedMessage.Length)));
                
                await SaveUserMessageAsync(sessionId, userId, sanitizedMessage);
                var rejectMessage = "I can only help with fitness and health-related requests.";
                var aiMessage = await SaveAiMessageAsync(sessionId, userId, rejectMessage);
                
                var limitsForUnsafe = await _messageLimitService.GetMessageLimitsAsync(userId);
                var creditBalanceForUnsafe = await _personalCoachApiService.GetCreditBalanceAsync(userId);
                    
                return new SendMessageResponse(
                    SessionId: sessionId,
                    Message: rejectMessage,
                    Role: "ai",
                    RemainingMessages: limitsForUnsafe.RemainingDaily,
                    DailyLimit: limitsForUnsafe.DailyLimit,
                    CreatedAt: aiMessage.CreatedAt,
                    RemainingCredits: creditBalanceForUnsafe?.RemainingCredits,
                    MonthlyCredits: creditBalanceForUnsafe?.MonthlyCredits,
                    SubscriptionTier: creditBalanceForUnsafe?.SubscriptionTier
                );
            }

            // Save user message
            await SaveUserMessageAsync(sessionId, userId, sanitizedMessage);

            string aiResponse;
            ActionResult? actionResult = null;
            bool requiresConfirmation = false;
            
            // Check if action requires confirmation or can be executed immediately
            if (intent.Type != IntentType.Normal && intent.RequiresConfirmation)
            {
                // Action requires confirmation - store it and ask user
                await StorePendingActionAsync(sessionId, intent);
                aiResponse = GenerateConfirmationMessage(intent);
                requiresConfirmation = true;
                
                _logger.LogInformation("Action {ActionType} requires confirmation for user {UserId}", 
                    intent.Type, userId);
            }
            else if (intent.Type != IntentType.Normal)
            {
                // Execute action immediately (safe operations)
                _logger.LogInformation("Executing action {ActionType} immediately for user {UserId}", 
                    intent.Type, userId);
                
                actionResult = await _actionExecutorService.ExecuteActionAsync(userId, intent, userContext);
                aiResponse = await GenerateActionResponseAsync(actionResult, userContext);
            }
            else
            {
                // Normal conversation - use Gemini
                var rawAiResponse = await _geminiService.GenerateResponseAsync(sanitizedMessage, userContext);
                aiResponse = _responseValidationService.ValidateAndSanitizeResponse(rawAiResponse);
            }

            // Save AI message
            var finalAiMessage = await SaveAiMessageAsync(sessionId, userId, aiResponse);

            // Check if summarization is needed
            await CheckAndSummarizeIfNeededAsync(sessionId);

            // Deduct credit for chat message
            await _messageLimitService.IncrementMessageCountAsync(userId);

            // Get updated limits and credit balance
            var finalLimits = await _messageLimitService.GetMessageLimitsAsync(userId);
            var finalCreditBalance = await _personalCoachApiService.GetCreditBalanceAsync(userId);

            return new SendMessageResponse(
                SessionId: sessionId,
                Message: aiResponse,
                Role: "ai",
                RemainingMessages: finalLimits.RemainingDaily,
                DailyLimit: finalLimits.DailyLimit,
                CreatedAt: finalAiMessage.CreatedAt,
                RemainingCredits: finalCreditBalance?.RemainingCredits,
                MonthlyCredits: finalCreditBalance?.MonthlyCredits,
                SubscriptionTier: finalCreditBalance?.SubscriptionTier,
                RequiresConfirmation: requiresConfirmation,
                PendingActionType: requiresConfirmation ? intent.Type.ToString() : null,
                PendingActionParams: requiresConfirmation ? intent.ExtractedParams : null,
                ActionResult: actionResult
            );
        }
        
        private async Task<UserContextDTO> GetOrBuildUserContextAsync(Guid userId, Guid sessionId)
        {
            // Try to get cached context
            var cachedContext = await _contextCacheService.GetCachedContextAsync(sessionId);
            
            if (cachedContext != null)
            {
                _logger.LogDebug("Using cached context for session {SessionId}", sessionId);
                return cachedContext;
            }

            // Build fresh context
            _logger.LogDebug("Building fresh context for session {SessionId}", sessionId);
            var context = await _userContextService.BuildUserContextAsync(userId, sessionId);
            
            // Cache it
            await _contextCacheService.SetCachedContextAsync(sessionId, context);
            
            return context;
        }

        public async Task<List<ChatMessageDTO>> GetSessionMessagesAsync(Guid sessionId)
        {
            var messages = await _messageRepository.GetBySessionIdAsync(sessionId);
            return messages.Select(m => new ChatMessageDTO(
                m.Id,
                m.SessionId,
                m.UserId,
                m.Role,
                m.Content,
                m.TokenCount,
                m.CreatedAt
            )).ToList();
        }

        public async Task<List<ChatSessionDTO>> GetUserSessionsAsync(Guid userId)
        {
            var sessions = await _sessionRepository.GetByUserIdAsync(userId);
            return sessions.Select(s => new ChatSessionDTO(
                s.Id,
                s.UserId,
                s.Title,
                s.Summary,
                s.Status,
                s.CreatedAt,
                s.UpdatedAt
            )).ToList();
        }

        public async Task<ChatSessionDTO> CreateSessionAsync(Guid userId, string? title = null)
        {
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title ?? "New Chat",
                Status = "open",
                CreatedAt = DateTime.UtcNow
            };

            await _sessionRepository.AddAsync(session);

            return new ChatSessionDTO(
                session.Id,
                session.UserId,
                session.Title,
                session.Summary,
                session.Status,
                session.CreatedAt,
                session.UpdatedAt
            );
        }

        public async Task CheckAndSummarizeIfNeededAsync(Guid sessionId)
        {
            var messageCount = await _messageRepository.GetMessageCountBySessionIdAsync(sessionId);
            
            // Check if we've reached the threshold
            if (messageCount >= _summarizationThreshold && messageCount % _summarizationThreshold == 0)
            {
                var messages = await _messageRepository.GetBySessionIdAsync(sessionId);
                
                // Build conversation history
                var historyBuilder = new StringBuilder();
                foreach (var msg in messages)
                {
                    historyBuilder.AppendLine($"{msg.Role}: {msg.Content}");
                }

                // Generate summary
                var summary = await _geminiService.SummarizeConversationAsync(historyBuilder.ToString());

                // Save summary
                await _summaryRepository.AddAsync(new ConversationSummary
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    SummaryText = summary,
                    MessageCount = messageCount,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        #region Action Execution Helper Methods

        private bool IsConfirmationResponse(string message)
        {
            var messageLower = message.ToLowerInvariant().Trim();
            return messageLower == "yes" || 
                   messageLower == "confirm" || 
                   messageLower == "ok" || 
                   messageLower == "sure" ||
                   messageLower == "go ahead" || 
                   messageLower == "do it" ||
                   messageLower == "please" ||
                   messageLower == "yes please";
        }

        private bool IsCancellationResponse(string message)
        {
            var messageLower = message.ToLowerInvariant().Trim();
            return messageLower == "no" || 
                   messageLower == "cancel" || 
                   messageLower == "stop" || 
                   messageLower == "nevermind" ||
                   messageLower == "never mind" || 
                   messageLower == "don't" ||
                   messageLower == "dont";
        }

        private async Task<IntentDetectionResult?> GetPendingActionAsync(Guid sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null || string.IsNullOrEmpty(session.UserContextSnapshot))
                return null;

            try
            {
                var context = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(session.UserContextSnapshot);
                if (context != null && context.ContainsKey("pendingAction"))
                {
                    var pendingActionJson = context["pendingAction"].ToString();
                    return System.Text.Json.JsonSerializer.Deserialize<IntentDetectionResult>(pendingActionJson ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending action for session {SessionId}", sessionId);
            }

            return null;
        }

        private async Task StorePendingActionAsync(Guid sessionId, IntentDetectionResult intent)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return;

            var context = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(session.UserContextSnapshot))
            {
                try
                {
                    context = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(session.UserContextSnapshot) ?? new Dictionary<string, object>();
                }
                catch { }
            }

            context["pendingAction"] = System.Text.Json.JsonSerializer.Serialize(intent);
            session.UserContextSnapshot = System.Text.Json.JsonSerializer.Serialize(context);
            
            await _sessionRepository.UpdateAsync(session);
        }

        private async Task ClearPendingActionAsync(Guid sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return;

            if (!string.IsNullOrEmpty(session.UserContextSnapshot))
            {
                try
                {
                    var context = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(session.UserContextSnapshot) ?? new Dictionary<string, object>();
                    context.Remove("pendingAction");
                    session.UserContextSnapshot = System.Text.Json.JsonSerializer.Serialize(context);
                    await _sessionRepository.UpdateAsync(session);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing pending action for session {SessionId}", sessionId);
                }
            }
        }

        private async Task<ChatMessage> SaveUserMessageAsync(Guid sessionId, Guid userId, string content)
        {
            var userMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                UserId = userId,
                Role = "user",
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(userMessage);
            return userMessage;
        }

        private async Task<ChatMessage> SaveAiMessageAsync(Guid sessionId, Guid userId, string content)
        {
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                UserId = userId,
                Role = "ai",
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(aiMessage);
            return aiMessage;
        }

        private string GenerateConfirmationMessage(IntentDetectionResult intent)
        {
            var action = intent.SuggestedAction ?? intent.Type.ToString();
            var sb = new StringBuilder();
            
            sb.AppendLine($"I'd like to {action.ToLower()} for you.");
            
            if (intent.ExtractedParams.Any())
            {
                sb.AppendLine("\n**Details:**");
                foreach (var param in intent.ExtractedParams.Take(5)) // Show max 5 params
                {
                    sb.AppendLine($"- {param.Key}: {param.Value}");
                }
            }
            
            sb.AppendLine("\n**Would you like me to proceed?**");
            sb.AppendLine("Reply with 'yes' to confirm or 'no' to cancel.");
            
            return sb.ToString();
        }

        private async Task<string> GenerateActionResponseAsync(ActionResult actionResult, UserContextDTO userContext)
        {
            if (actionResult.Success)
            {
                // Use Gemini to make the success message more conversational if needed
                if (actionResult.Data != null)
                {
                    var prompt = $"A user requested an action and it succeeded. Here's the result:\n\n{actionResult.Message}\n\nData: {System.Text.Json.JsonSerializer.Serialize(actionResult.Data)}\n\nMake this response more conversational and friendly, highlighting the key points. Keep it concise.";
                    
                    try
                    {
                        var enhancedMessage = await _geminiService.GenerateResponseAsync(prompt, userContext);
                        return _responseValidationService.ValidateAndSanitizeResponse(enhancedMessage);
                    }
                    catch
                    {
                        // Fallback to original message
                        return actionResult.Message;
                    }
                }
                
                return actionResult.Message;
            }
            else
            {
                // For errors, use the error message as-is (already user-friendly from ActionExecutor)
                return actionResult.Message;
            }
        }

        #endregion
    }
}

