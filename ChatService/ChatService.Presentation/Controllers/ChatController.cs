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

        public ChatController(
            IConversationService conversationService,
            IMessageLimitService messageLimitService)
        {
            _conversationService = conversationService;
            _messageLimitService = messageLimitService;
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
                return StatusCode(500, new { message = "An error occurred while processing your message", error = ex.Message });
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
    }

    public record CreateSessionRequest(string? Title);
}

