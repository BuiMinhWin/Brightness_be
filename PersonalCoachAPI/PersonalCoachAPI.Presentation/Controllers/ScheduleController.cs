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
        private readonly IManualScheduleService _manualScheduleService;
        private readonly IAIUsageLimitService _aiUsageLimitService;
        private readonly IAICreditService _aiCreditService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IAIScheduleService scheduleService,
            IManualScheduleService manualScheduleService,
            IAIUsageLimitService aiUsageLimitService,
            IAICreditService aiCreditService,
            ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _manualScheduleService = manualScheduleService;
            _aiUsageLimitService = aiUsageLimitService;
            _aiCreditService = aiCreditService;
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
            catch (InvalidOperationException ex) when (ex.Message.Contains("limit reached"))
            {
                _logger.LogWarning(ex, "AI generation limit reached for user");
                return StatusCode(429, new { success = false, message = ex.Message, code = "AI_LIMIT_REACHED" });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient") || ex.Message.Contains("credits"))
            {
                _logger.LogWarning(ex, "Insufficient credits for schedule generation");
                return StatusCode(402, new { 
                    success = false,
                    message = ex.Message,
                    code = "INSUFFICIENT_CREDITS"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating schedule");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check for conflicts with existing workouts before generating new schedule
        /// </summary>
        [HttpPost("check-conflicts")]
        public async Task<ActionResult<ScheduleConflictInfo>> CheckConflicts(
            [FromBody] CheckConflictsRequest request)
        {
            try
            {
                var result = await _scheduleService.CheckScheduleConflictsAsync(
                    request.UserId,
                    request.StartDate,
                    request.DurationDays,
                    request.AvailableDays);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking schedule conflicts");
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

        // =====================================================
        // MANUAL SCHEDULE BUILDER ENDPOINTS
        // =====================================================

        /// <summary>
        /// Get AI usage limits for current user (Legacy - use ai-credit-balance instead)
        /// </summary>
        [HttpGet("ai-usage-limits/{userId}")]
        public async Task<ActionResult<AIUsageLimitsDTO>> GetAIUsageLimits(Guid userId)
        {
            try
            {
                var limits = await _aiUsageLimitService.GetUsageLimitsAsync(userId);
                return Ok(new { success = true, data = limits });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI usage limits");
                return StatusCode(500, new { success = false, message = "Failed to get usage limits" });
            }
        }

        /// <summary>
        /// Get AI credit balance for current user (NEW credit-based system)
        /// </summary>
        [HttpGet("ai-credit-balance/{userId}")]
        public async Task<ActionResult<AICreditBalanceDTO>> GetAICreditBalance(Guid userId)
        {
            try
            {
                var balance = await _aiCreditService.GetCreditBalanceAsync(userId);
                return Ok(new { success = true, data = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI credit balance");
                return StatusCode(500, new { success = false, message = "Failed to get credit balance" });
            }
        }

        /// <summary>
        /// Get AI credit usage history for current user
        /// </summary>
        [HttpGet("ai-credit-history/{userId}")]
        public async Task<ActionResult<List<CreditHistoryItemDTO>>> GetAICreditHistory(Guid userId, [FromQuery] int limit = 50)
        {
            try
            {
                var history = await _aiCreditService.GetCreditHistoryAsync(userId, limit);
                return Ok(new { success = true, data = history, count = history.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI credit history");
                return StatusCode(500, new { success = false, message = "Failed to get credit history" });
            }
        }

        /// <summary>
        /// Deduct credits for a specific AI operation (used by other microservices)
        /// </summary>
        [HttpPost("deduct-credit")]
        public async Task<IActionResult> DeductCredit([FromBody] DeductCreditRequest request)
        {
            try
            {
                await _aiCreditService.DeductCreditsAsync(request.UserId, request.OperationType);
                var balance = await _aiCreditService.GetCreditBalanceAsync(request.UserId);
                return Ok(new { 
                    success = true, 
                    message = "Credit deducted successfully",
                    remainingCredits = balance.RemainingCredits
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient credits"))
            {
                _logger.LogWarning(ex, "Insufficient credits for user {UserId}", request.UserId);
                return StatusCode(402, new { success = false, message = ex.Message, code = "INSUFFICIENT_CREDITS" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting credit for user {UserId}", request.UserId);
                return StatusCode(500, new { success = false, message = "Failed to deduct credit" });
            }
        }

        /// <summary>
        /// Create manual workout schedule (no AI generation)
        /// </summary>
        [HttpPost("manual")]
        public async Task<ActionResult<WorkoutScheduleResponse>> CreateManualSchedule(
            [FromBody] ManualScheduleRequest request)
        {
            try
            {
                var result = await _manualScheduleService.CreateManualScheduleAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual schedule");
                return StatusCode(500, new { success = false, message = "Failed to create manual schedule" });
            }
        }

        /// <summary>
        /// Add workout day from template to schedule
        /// </summary>
        [HttpPost("{scheduleId}/days/from-template")]
        public async Task<ActionResult<WorkoutDayResponse>> AddDayFromTemplate(
            Guid scheduleId,
            [FromBody] AddDayFromTemplateRequest request)
        {
            try
            {
                request = request with { ScheduleId = scheduleId };
                var result = await _manualScheduleService.AddWorkoutDayFromTemplateAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding day from template");
                return StatusCode(500, new { success = false, message = "Failed to add day from template" });
            }
        }

        /// <summary>
        /// Add custom workout day to schedule
        /// </summary>
        [HttpPost("{scheduleId}/days/custom")]
        public async Task<ActionResult<WorkoutDayResponse>> AddCustomDay(
            Guid scheduleId,
            [FromBody] AddCustomDayRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid AddCustomDayRequest model: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });
            }
            
            try
            {
                _logger.LogInformation("Adding custom day to schedule {ScheduleId}, Date: {Date}", scheduleId, request.Date);
                request = request with { ScheduleId = scheduleId };
                var result = await _manualScheduleService.AddCustomWorkoutDayAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding custom day");
                return StatusCode(500, new { success = false, message = "Failed to add custom day" });
            }
        }

        /// <summary>
        /// Update workout day exercises
        /// </summary>
        [HttpPut("{scheduleId}/days/{dayId}")]
        public async Task<ActionResult<WorkoutDayResponse>> UpdateWorkoutDay(
            Guid scheduleId,
            Guid dayId,
            [FromBody] UpdateWorkoutDayRequest request)
        {
            try
            {
                var result = await _manualScheduleService.UpdateWorkoutDayAsync(scheduleId, dayId, request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workout day");
                return StatusCode(500, new { success = false, message = "Failed to update workout day" });
            }
        }

        /// <summary>
        /// Delete workout day from schedule
        /// </summary>
        [HttpDelete("{scheduleId}/days/{dayId}")]
        public async Task<IActionResult> DeleteWorkoutDay(Guid scheduleId, Guid dayId)
        {
            try
            {
                await _manualScheduleService.DeleteWorkoutDayAsync(scheduleId, dayId);
                return Ok(new { success = true, message = "Workout day deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workout day");
                return StatusCode(500, new { success = false, message = "Failed to delete workout day" });
            }
        }
    }
}

