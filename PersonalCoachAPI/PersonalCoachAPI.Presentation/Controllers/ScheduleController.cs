using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IAIScheduleService _scheduleService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IAIScheduleService scheduleService, ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        /// <summary>
        /// Generate AI-powered workout schedule (FR-007)
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<WorkoutScheduleResponse>> GenerateSchedule(
            [FromBody] GenerateScheduleRequest request)
        {
            try
            {
                var result = await _scheduleService.GenerateScheduleAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating schedule");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Modify specific day in schedule without regenerating all
        /// </summary>
        [HttpPatch("{scheduleId}/day/{dayNumber}")]
        public async Task<ActionResult<ModifiedDayResponse>> ModifyDay(
            Guid scheduleId,
            int dayNumber,
            [FromBody] string userRequest)
        {
            try
            {
                var request = new ModifyDayRequest
                {
                    ScheduleId = scheduleId,
                    DayNumber = dayNumber,
                    UserRequest = userRequest
                };

                var result = await _scheduleService.ModifyScheduleDayAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error modifying schedule day");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Adjust schedule difficulty based on feedback (FR-009)
        /// </summary>
        [HttpPost("{scheduleId}/adjust")]
        public async Task<ActionResult<ScheduleAdjustmentResponse>> AdjustDifficulty(
            Guid scheduleId,
            [FromBody] AdjustDifficultyRequest request)
        {
            try
            {
                request.ScheduleId = scheduleId;
                var result = await _scheduleService.AdjustScheduleDifficultyAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting schedule");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get schedule details
        /// </summary>
        [HttpGet("{scheduleId}")]
        public async Task<ActionResult<WorkoutScheduleResponse>> GetSchedule(Guid scheduleId)
        {
            try
            {
                var result = await _scheduleService.GetScheduleAsync(scheduleId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all schedules for a user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<WorkoutScheduleResponse>>> GetUserSchedules(Guid userId)
        {
            try
            {
                var result = await _scheduleService.GetUserSchedulesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user schedules");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Mark exercise as completed
        /// </summary>
        [HttpPatch("exercise/{exerciseId}/complete")]
        public async Task<ActionResult> MarkExerciseComplete(
            Guid exerciseId,
            [FromBody] CompleteExerciseRequest request)
        {
            try
            {
                await _scheduleService.MarkExerciseCompleteAsync(exerciseId, request.Completed, request.ActualSets, request.ActualReps);
                return Ok(new { message = "Exercise updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking exercise complete");
                return StatusCode(500, "Internal server error");
            }
        }

        // ============================================
        // FULL SUITE ENDPOINTS
        // ============================================

        /// <summary>
        /// Submit workout feedback for a specific day
        /// </summary>
        [HttpPost("feedback")]
        public async Task<ActionResult> SubmitFeedback([FromBody] SubmitWorkoutFeedbackRequest request)
        {
            try
            {
                await _scheduleService.SubmitWorkoutFeedbackAsync(request);
                return Ok(new { message = "Feedback submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Regenerate schedule from a specific day forward, preserving completed progress
        /// </summary>
        [HttpPost("regenerate")]
        public async Task<ActionResult<WorkoutScheduleResponse>> RegenerateSchedule(
            [FromBody] RegenerateScheduleRequest request)
        {
            try
            {
                var result = await _scheduleService.RegenerateScheduleFromDayAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating schedule");
                return StatusCode(500, new { message = "Error regenerating schedule", error = ex.Message });
            }
        }

        /// <summary>
        /// Edit a specific workout day
        /// </summary>
        [HttpPatch("day/edit")]
        public async Task<ActionResult<ScheduleDayDetail>> EditWorkoutDay(
            [FromBody] EditWorkoutDayRequest request)
        {
            try
            {
                var result = await _scheduleService.EditWorkoutDayAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing workout day");
                return StatusCode(500, new { message = "Error editing workout day", error = ex.Message });
            }
        }

        /// <summary>
        /// Get schedule history and versions for a user
        /// </summary>
        [HttpGet("history/user/{userId}")]
        public async Task<ActionResult<ScheduleHistoryResponse>> GetScheduleHistory(Guid userId)
        {
            try
            {
                var result = await _scheduleService.GetScheduleHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule history");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get analytics for user's workout patterns
        /// </summary>
        [HttpGet("analytics/user/{userId}")]
        public async Task<ActionResult<ScheduleAnalytics>> GetAnalytics(Guid userId)
        {
            try
            {
                var result = await _scheduleService.GetScheduleAnalyticsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

