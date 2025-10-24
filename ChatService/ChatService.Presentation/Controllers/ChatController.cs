using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatService.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IMessageLimitService _messageLimitService;
        private readonly IPersonalCoachApiService _personalCoachApiService;
        private readonly IUserContextCacheService _contextCacheService;
        private readonly IProfileAnalysisService _profileAnalysisService;

        public ChatController(
            IConversationService conversationService,
            IMessageLimitService messageLimitService,
            IPersonalCoachApiService personalCoachApiService,
            IUserContextCacheService contextCacheService,
            IProfileAnalysisService profileAnalysisService)
        {
            _conversationService = conversationService;
            _messageLimitService = messageLimitService;
            _personalCoachApiService = personalCoachApiService;
            _contextCacheService = contextCacheService;
            _profileAnalysisService = profileAnalysisService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            return userId;
        }

        /// <summary>
        /// Send a message and get AI response
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _conversationService.SendMessageAsync(userId, request);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("limit reached"))
            {
                return StatusCode(429, new { message = ex.Message, limitReached = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in SendMessage: {ex}");
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
                return StatusCode(500, new { message = "An error occurred while processing your message", error = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Get user's chat sessions
        /// </summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var userId = GetUserId();
                var sessions = await _conversationService.GetUserSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching sessions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get messages for a specific session
        /// </summary>
        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            try
            {
                var messages = await _conversationService.GetSessionMessagesAsync(sessionId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching messages", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new chat session
        /// </summary>
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest? request = null)
        {
            try
            {
                var userId = GetUserId();
                var session = await _conversationService.CreateSessionAsync(userId, request?.Title);
                return Ok(session);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating session", error = ex.Message });
            }
        }

        /// <summary>
        /// Get remaining message limits
        /// </summary>
        [HttpGet("limits")]
        public async Task<IActionResult> GetLimits()
        {
            try
            {
                var userId = GetUserId();
                var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
                return Ok(limits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching limits", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a workout schedule from chat
        /// </summary>
        [HttpPost("create-schedule")]
        public async Task<IActionResult> CreateScheduleFromChat([FromBody] CreateScheduleFromChatRequest request)
        {
            try
            {
                var userId = GetUserId(); // From JWT, NOT request body
                
                // Validate request DTO (data annotations)
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                // Check message limits
                var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
                if (limits.IsLimitReached)
                {
                    return StatusCode(429, new { message = "Daily message limit reached", limitReached = true });
                }

                // Count this AI operation as a message
                await _messageLimitService.IncrementMessageCountAsync(userId);
                
                var scheduleResponse = await _personalCoachApiService.CreateScheduleAsync(userId, request);
                
                // Invalidate cached context since schedule was created
                await _contextCacheService.InvalidateCacheAsync(request.SessionId);
                
                // Get updated limits
                var newLimits = await _messageLimitService.GetMessageLimitsAsync(userId);
                
                return Ok(new
                {
                    schedule = scheduleResponse,
                    remainingMessages = newLimits.RemainingDaily,
                    dailyLimit = newLimits.DailyLimit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user preferences from chat
        /// </summary>
        [HttpPost("update-preference")]
        public async Task<IActionResult> UpdatePreference([FromBody] PreferenceUpdateDTO request)
        {
            try
            {
                var userId = GetUserId(); // From JWT, NOT request body
                
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                // Check message limits
                var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
                if (limits.IsLimitReached)
                {
                    return StatusCode(429, new { message = "Daily message limit reached", limitReached = true });
                }

                // Count this AI operation as a message
                await _messageLimitService.IncrementMessageCountAsync(userId);
                
                // Note: The actual preference update is handled by the frontend calling PersonalCoach API directly
                // This endpoint just tracks AI credits and invalidates cache
                
                // Invalidate cached context since preferences updated
                await _contextCacheService.InvalidateCacheAsync(request.SessionId);
                
                // Get updated limits
                var newLimits = await _messageLimitService.GetMessageLimitsAsync(userId);
                
                return Ok(new
                {
                    message = "Preference update tracked successfully",
                    remainingMessages = newLimits.RemainingDaily,
                    dailyLimit = newLimits.DailyLimit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating preference", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's workout schedules
        /// </summary>
        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            try
            {
                var userId = GetUserId();
                var schedules = await _personalCoachApiService.GetUserSchedulesAsync(userId);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching schedules", error = ex.Message });
            }
        }

        /// <summary>
        /// Analyze user profile and return AI-recommended schedule parameters
        /// </summary>
        [HttpGet("analyze-profile")]
        public async Task<IActionResult> AnalyzeProfile()
        {
            try
            {
                var userId = GetUserId();
                var recommendation = await _profileAnalysisService.AnalyzeUserProfileAsync(userId);
                
                return Ok(new
                {
                    goal = recommendation.Goal,
                    difficultyLevel = recommendation.DifficultyLevel,
                    durationDays = recommendation.DurationDays,
                    reasoning = recommendation.Reasoning
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while analyzing profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Track AI action (increment message count and return limits)
        /// </summary>
        [HttpPost("track-ai-action")]
        public async Task<IActionResult> TrackAiAction()
        {
            try
            {
                var userId = GetUserId();
                
                // Check message limits
                var limits = await _messageLimitService.GetMessageLimitsAsync(userId);
                if (limits.IsLimitReached)
                {
                    return StatusCode(429, new { message = "Daily message limit reached", limitReached = true });
                }

                // Count this AI operation as a message
                await _messageLimitService.IncrementMessageCountAsync(userId);
                
                // Get updated limits
                var newLimits = await _messageLimitService.GetMessageLimitsAsync(userId);
                
                return Ok(new
                {
                    remainingMessages = newLimits.RemainingDaily,
                    dailyLimit = newLimits.DailyLimit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while tracking AI action", error = ex.Message });
            }
        }

        /// <summary>
        /// Invalidate user context cache (called after profile updates)
        /// </summary>
        [HttpPost("invalidate-cache")]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                var userId = GetUserId();
                await _contextCacheService.InvalidateUserCacheAsync(userId);
                return Ok(new { message = "Cache invalidated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while invalidating cache", error = ex.Message });
            }
        }
    }

    public record CreateSessionRequest(string? Title);
}

