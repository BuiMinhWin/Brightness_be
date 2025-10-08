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
    }
}

