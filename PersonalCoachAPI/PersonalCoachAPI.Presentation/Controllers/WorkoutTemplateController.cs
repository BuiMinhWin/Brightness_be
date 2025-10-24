using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/workout-templates")]
    [Authorize]
    public class WorkoutTemplateController : ControllerBase
    {
        private readonly IWorkoutTemplateService _templateService;
        private readonly ILogger<WorkoutTemplateController> _logger;

        public WorkoutTemplateController(
            IWorkoutTemplateService templateService,
            ILogger<WorkoutTemplateController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        /// <summary>
        /// Get all workout templates with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTemplates(
            [FromQuery] string? focusArea = null,
            [FromQuery] string? difficulty = null)
        {
            try
            {
                var filters = new TemplateFilters(
                    FocusArea: focusArea,
                    Difficulty: difficulty
                );

                var templates = await _templateService.GetAllTemplatesAsync(filters);
                
                return Ok(new
                {
                    success = true,
                    data = templates,
                    count = templates.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates");
                return StatusCode(500, new { success = false, message = "Failed to get templates" });
            }
        }

        /// <summary>
        /// Get template by ID with full exercise details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            try
            {
                var template = await _templateService.GetTemplateByIdAsync(id);
                
                if (template == null)
                {
                    return NotFound(new { success = false, message = "Template not found" });
                }

                return Ok(new { success = true, data = template });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template {TemplateId}", id);
                return StatusCode(500, new { success = false, message = "Failed to get template" });
            }
        }
    }
}




