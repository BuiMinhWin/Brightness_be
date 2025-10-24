using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/exercise-library")]
    [Authorize]
    public class ExerciseLibraryController : ControllerBase
    {
        private readonly IExerciseLibraryService _exerciseLibraryService;
        private readonly ILogger<ExerciseLibraryController> _logger;

        public ExerciseLibraryController(
            IExerciseLibraryService exerciseLibraryService,
            ILogger<ExerciseLibraryController> logger)
        {
            _exerciseLibraryService = exerciseLibraryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all exercises with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllExercises(
            [FromQuery] string? category = null,
            [FromQuery] string? difficulty = null,
            [FromQuery] string? searchQuery = null)
        {
            try
            {
                var filters = new ExerciseFilters(
                    Category: category,
                    Difficulty: difficulty,
                    EquipmentAvailable: null,
                    SearchQuery: searchQuery
                );

                var exercises = await _exerciseLibraryService.GetAllExercisesAsync(filters);
                
                return Ok(new
                {
                    success = true,
                    data = exercises,
                    count = exercises.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exercises");
                return StatusCode(500, new { success = false, message = "Failed to get exercises" });
            }
        }

        /// <summary>
        /// Get exercise by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExerciseById(Guid id)
        {
            try
            {
                var exercise = await _exerciseLibraryService.GetExerciseByIdAsync(id);
                
                if (exercise == null)
                {
                    return NotFound(new { success = false, message = "Exercise not found" });
                }

                return Ok(new { success = true, data = exercise });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exercise {ExerciseId}", id);
                return StatusCode(500, new { success = false, message = "Failed to get exercise" });
            }
        }

        /// <summary>
        /// Search exercises by query
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchExercises([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { success = false, message = "Search query is required" });
                }

                var exercises = await _exerciseLibraryService.SearchExercisesAsync(q);
                
                return Ok(new
                {
                    success = true,
                    data = exercises,
                    count = exercises.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching exercises with query {Query}", q);
                return StatusCode(500, new { success = false, message = "Failed to search exercises" });
            }
        }

        /// <summary>
        /// Get exercise categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _exerciseLibraryService.GetCategoriesAsync();
                
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { success = false, message = "Failed to get categories" });
            }
        }

        /// <summary>
        /// Get AI exercise suggestions (doesn't consume AI quota)
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetAISuggestions(
            [FromQuery] string focusArea,
            [FromQuery] string difficulty,
            [FromQuery] int count = 5)
        {
            try
            {
                var request = new ExerciseSuggestionRequest(
                    FocusArea: focusArea,
                    Difficulty: difficulty,
                    EquipmentAvailable: null,
                    Count: count
                );

                var suggestions = await _exerciseLibraryService.GetAISuggestionsAsync(request);
                
                return Ok(new
                {
                    success = true,
                    data = suggestions,
                    count = suggestions.Count,
                    note = "AI suggestions don't consume your monthly AI generation quota"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI suggestions");
                return StatusCode(500, new { success = false, message = "Failed to get suggestions" });
            }
        }
    }
}




