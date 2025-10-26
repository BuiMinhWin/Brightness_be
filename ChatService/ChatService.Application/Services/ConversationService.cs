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

            // Run intent detection
            var intent = await _intentDetectionService.DetectIntentAsync(sanitizedMessage);
            
            // If unsafe intent detected, reject
            if (!intent.IsSafe)
            {
                _logger.LogWarning("Unsafe intent detected for user {UserId}: {Message}", 
                    userId, sanitizedMessage.Substring(0, Math.Min(100, sanitizedMessage.Length)));
                
                var limitsForUnsafe = await _messageLimitService.GetMessageLimitsAsync(userId);
                var creditBalanceForUnsafe = await _personalCoachApiService.GetCreditBalanceAsync(userId);
                    
                return new SendMessageResponse(
                    SessionId: sessionId,
                    Message: "I can only help with fitness and health-related requests.",
                    Role: "ai",
                    RemainingMessages: limitsForUnsafe.RemainingDaily,
                    DailyLimit: limitsForUnsafe.DailyLimit,
                    CreatedAt: DateTime.UtcNow,
                    RemainingCredits: creditBalanceForUnsafe?.RemainingCredits,
                    MonthlyCredits: creditBalanceForUnsafe?.MonthlyCredits,
                    SubscriptionTier: creditBalanceForUnsafe?.SubscriptionTier
                );
            }

            // Save user message
            var userMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                UserId = userId,
                Role = "user",
                Content = sanitizedMessage,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(userMessage);

            // Build user context (with caching)
            var userContext = await GetOrBuildUserContextAsync(userId, sessionId);

            // Generate AI response
            var rawAiResponse = await _geminiService.GenerateResponseAsync(sanitizedMessage, userContext);
            
            // Validate and sanitize AI response
            var aiResponse = _responseValidationService.ValidateAndSanitizeResponse(rawAiResponse);

            // Save AI message
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                UserId = userId,
                Role = "ai",
                Content = aiResponse,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(aiMessage);

            // Check if summarization is needed
            await CheckAndSummarizeIfNeededAsync(sessionId);

            // Deduct credit for chat message
            await _messageLimitService.IncrementMessageCountAsync(userId);

            // Get updated limits (legacy)
            var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
            
            // Get credit balance (new credit system)
            var creditBalance = await _personalCoachApiService.GetCreditBalanceAsync(userId);

            return new SendMessageResponse(
                SessionId: sessionId,
                Message: aiResponse,
                Role: "ai",
                RemainingMessages: limits.RemainingDaily,
                DailyLimit: limits.DailyLimit,
                CreatedAt: aiMessage.CreatedAt,
                RemainingCredits: creditBalance?.RemainingCredits,
                MonthlyCredits: creditBalance?.MonthlyCredits,
                SubscriptionTier: creditBalance?.SubscriptionTier
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
    }
}

