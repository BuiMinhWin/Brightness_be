using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleHistoryController : ControllerBase
    {
        private readonly IScheduleHistoryService _historyService;
        private readonly ILogger<ScheduleHistoryController> _logger;

        public ScheduleHistoryController(
            IScheduleHistoryService historyService,
            ILogger<ScheduleHistoryController> logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all schedule history for a user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ScheduleHistoryEntry>>> GetUserHistory(
            Guid userId,
            [FromQuery] int limit = 50)
        {
            try
            {
                var history = await _historyService.GetUserScheduleHistoryAsync(userId, limit);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching history for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to fetch history" });
            }
        }

        /// <summary>
        /// Get version history for a specific schedule
        /// </summary>
        [HttpGet("schedule/{scheduleId}/versions")]
        public async Task<ActionResult<List<ScheduleHistoryEntry>>> GetScheduleVersions(
            Guid scheduleId)
        {
            try
            {
                var versions = await _historyService.GetScheduleVersionHistoryAsync(scheduleId);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching versions for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { success = false, message = "Failed to fetch versions" });
            }
        }

        /// <summary>
        /// Get analytics for a user's schedule history
        /// </summary>
        [HttpGet("analytics/{userId}")]
        public async Task<ActionResult<UserScheduleAnalytics>> GetAnalytics(Guid userId)
        {
            try
            {
                var analytics = await _historyService.GetScheduleAnalyticsAsync(userId);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching analytics for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to fetch analytics" });
            }
        }

        /// <summary>
        /// Revert to a previous schedule version
        /// </summary>
        [HttpPost("revert/{scheduleId}")]
        public async Task<ActionResult<WorkoutScheduleResponse>> RevertToVersion(
            Guid scheduleId,
            [FromBody] RevertVersionRequest request)
        {
            try
            {
                var schedule = await _historyService.RevertToVersionAsync(scheduleId, request.UserId);
                return Ok(schedule);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid revert request for schedule {ScheduleId}", scheduleId);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reverting to schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { success = false, message = "Failed to revert schedule" });
            }
        }
    }

    public class RevertVersionRequest
    {
        public Guid UserId { get; set; }
    }
}

