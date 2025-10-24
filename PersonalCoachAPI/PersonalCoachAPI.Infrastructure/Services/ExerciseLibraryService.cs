using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class ExerciseLibraryService : IExerciseLibraryService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<ExerciseLibraryService> _logger;
        private readonly IRAGService _ragService;
        private readonly IAIUsageLimitService _aiUsageLimitService;

        public ExerciseLibraryService(
            PersonalCoachDbContext context,
            ILogger<ExerciseLibraryService> logger,
            IRAGService ragService,
            IAIUsageLimitService aiUsageLimitService)
        {
            _context = context;
            _logger = logger;
            _ragService = ragService;
            _aiUsageLimitService = aiUsageLimitService;
        }

        public async Task<List<ExerciseLibraryDTO>> GetAllExercisesAsync(ExerciseFilters? filters = null)
        {
            var query = _context.ExerciseLibrary.Where(e => e.IsActive);

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.Category))
                {
                    query = query.Where(e => e.Category == filters.Category);
                }

                if (!string.IsNullOrEmpty(filters.Difficulty))
                {
                    query = query.Where(e => e.DifficultyLevel == filters.Difficulty);
                }

                if (!string.IsNullOrEmpty(filters.SearchQuery))
                {
                    var searchLower = filters.SearchQuery.ToLower();
                    query = query.Where(e => 
                        EF.Functions.ILike(e.ExerciseName, $"%{searchLower}%") ||
                        EF.Functions.ILike(e.ExerciseNameVi, $"%{searchLower}%") ||
                        EF.Functions.ILike(e.Description, $"%{searchLower}%"));
                }

                if (filters.EquipmentAvailable != null && filters.EquipmentAvailable.Any())
                {
                    // Filter exercises that only require equipment from the available list
                    // This requires checking JSON arrays - simplified version
                    query = query.Where(e => e.EquipmentRequired == "[]" || e.EquipmentRequired == "");
                }
            }

            var exercises = await query.OrderBy(e => e.Category).ThenBy(e => e.ExerciseName).ToListAsync();

            return exercises.Select(MapToDTO).ToList();
        }

        public async Task<ExerciseLibraryDTO?> GetExerciseByIdAsync(Guid id)
        {
            var exercise = await _context.ExerciseLibrary.FirstOrDefaultAsync(e => e.Id == id);
            return exercise != null ? MapToDTO(exercise) : null;
        }

        public async Task<List<ExerciseLibraryDTO>> SearchExercisesAsync(string query)
        {
            var searchLower = query.ToLower();
            // Search only in text fields (ExerciseName, ExerciseNameVi, Description)
            // TargetMuscles is JSONB and complex to query directly
            var exercises = await _context.ExerciseLibrary
                .Where(e => e.IsActive)
                .Where(e => 
                    EF.Functions.ILike(e.ExerciseName, $"%{searchLower}%") ||
                    EF.Functions.ILike(e.ExerciseNameVi, $"%{searchLower}%") ||
                    EF.Functions.ILike(e.Description, $"%{searchLower}%"))
                .Take(50)
                .ToListAsync();

            return exercises.Select(MapToDTO).ToList();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var categories = await _context.ExerciseLibrary
                .Where(e => e.IsActive)
                .Select(e => e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return categories;
        }

        public async Task<List<ExerciseLibraryDTO>> GetAISuggestionsAsync(ExerciseSuggestionRequest request)
        {
            // Check AI usage limits before generating suggestions
            var usageLimits = await _aiUsageLimitService.GetUsageLimitsAsync(request.UserId);
            if (!usageLimits.CanGenerate)
            {
                _logger.LogWarning(
                    "User {UserId} has reached AI generation limit ({Used}/{Limit})",
                    request.UserId, usageLimits.UsedThisMonth, usageLimits.MonthlyLimit);
                    
                throw new InvalidOperationException(
                    $"Monthly AI generation limit reached ({usageLimits.MonthlyLimit}). " +
                    $"Resets in {usageLimits.DaysUntilReset} days. Upgrade to premium for unlimited access.");
            }

            _logger.LogInformation(
                "Using AI to generate {Count} exercise suggestions for user {UserId}, focus: {FocusArea}, difficulty: {Difficulty}",
                request.Count, request.UserId, request.FocusArea, request.Difficulty);

            try
            {
                // Handle "Let AI Decide" options - use more flexible parameters
                var focusArea = request.FocusArea == "ai_decide" 
                    ? "varied workout based on user goals and fitness level" 
                    : request.FocusArea;
                    
                var difficulty = request.Difficulty == "ai_decide" 
                    ? "appropriate for user fitness level" 
                    : request.Difficulty;

                // Use AI to generate personalized exercise suggestions
                var aiExercises = await _ragService.GenerateExercisesWithAIAsync(
                    focusArea,
                    difficulty,
                    request.EquipmentAvailable ?? new List<string>(),
                    request.Count);

                // Track AI usage
                await _aiUsageLimitService.TrackGenerationAsync(request.UserId, "AI Exercise Suggestions");

                _logger.LogInformation(
                    "Successfully generated {Count} AI exercise suggestions for user {UserId}",
                    aiExercises.Count, request.UserId);

                // Map ExerciseSearchResult to ExerciseLibraryDTO
                return aiExercises.Select(ex => new ExerciseLibraryDTO(
                    ex.Id,
                    ex.ExerciseName,
                    ex.ExerciseNameVi,
                    ex.Category,
                    ex.TargetMuscles,
                    ex.DifficultyLevel,
                    ex.EquipmentRequired,
                    ex.Description,
                    ex.VideoUrl,
                    DefaultSets: 3,        // Default values for AI-generated suggestions
                    DefaultReps: "10-12",
                    DefaultRestSeconds: 60
                )).ToList();
            }
            catch (InvalidOperationException)
            {
                // Re-throw AI limit exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error generating AI exercise suggestions for user {UserId}. Falling back to smart filtering.",
                    request.UserId);

                // Fallback to smart filtering if AI fails
                return await GetFallbackSuggestionsAsync(request);
            }
        }

        private async Task<List<ExerciseLibraryDTO>> GetFallbackSuggestionsAsync(ExerciseSuggestionRequest request)
        {
            // Fallback: Use smart database filtering if AI fails
            var query = _context.ExerciseLibrary.Where(e => e.IsActive);

            // Filter by focus area using keyword matching
            if (!string.IsNullOrEmpty(request.FocusArea))
            {
                var focusLower = request.FocusArea.ToLower();
                
                switch (focusLower)
                {
                    case "cardio":
                        query = query.Where(e => e.Category == "cardio");
                        break;
                    case "upper_body":
                        query = query.Where(e => 
                            EF.Functions.ILike(e.ExerciseName, "%chest%") ||
                            EF.Functions.ILike(e.ExerciseName, "%shoulder%") ||
                            EF.Functions.ILike(e.ExerciseName, "%back%") ||
                            EF.Functions.ILike(e.ExerciseName, "%arm%") ||
                            EF.Functions.ILike(e.ExerciseName, "%bicep%") ||
                            EF.Functions.ILike(e.ExerciseName, "%tricep%"));
                        break;
                    case "lower_body":
                        query = query.Where(e => 
                            EF.Functions.ILike(e.ExerciseName, "%leg%") ||
                            EF.Functions.ILike(e.ExerciseName, "%squat%") ||
                            EF.Functions.ILike(e.ExerciseName, "%lunge%") ||
                            EF.Functions.ILike(e.ExerciseName, "%glute%") ||
                            EF.Functions.ILike(e.ExerciseName, "%calf%") ||
                            EF.Functions.ILike(e.ExerciseName, "%thigh%"));
                        break;
                    case "core":
                        query = query.Where(e => 
                            EF.Functions.ILike(e.ExerciseName, "%core%") ||
                            EF.Functions.ILike(e.ExerciseName, "%ab%") ||
                            EF.Functions.ILike(e.ExerciseName, "%plank%") ||
                            EF.Functions.ILike(e.ExerciseName, "%crunch%"));
                        break;
                    case "full_body":
                        // No filtering for full body
                        break;
                }
            }

            // Filter by difficulty
            if (!string.IsNullOrEmpty(request.Difficulty))
            {
                query = query.Where(e => e.DifficultyLevel == request.Difficulty);
            }

            // Filter by equipment
            if (request.EquipmentAvailable != null && request.EquipmentAvailable.Any())
            {
                query = query.Where(e => e.EquipmentRequired == "[]" || e.EquipmentRequired == "");
            }

            var exercises = await query
                .OrderBy(e => Guid.NewGuid())
                .Take(request.Count)
                .ToListAsync();

            _logger.LogInformation(
                "Fallback: Generated {Count} suggestions for {FocusArea} / {Difficulty}",
                exercises.Count, request.FocusArea, request.Difficulty);

            return exercises.Select(MapToDTO).ToList();
        }

        private static ExerciseLibraryDTO MapToDTO(Domain.Entities.ExerciseLibrary exercise)
        {
            var targetMuscles = string.IsNullOrEmpty(exercise.TargetMuscles) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(exercise.TargetMuscles) ?? new List<string>();

            var equipment = string.IsNullOrEmpty(exercise.EquipmentRequired) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(exercise.EquipmentRequired) ?? new List<string>();

            return new ExerciseLibraryDTO(
                Id: exercise.Id,
                ExerciseName: exercise.ExerciseName,
                ExerciseNameVi: exercise.ExerciseNameVi,
                Category: exercise.Category,
                TargetMuscles: targetMuscles,
                Difficulty: exercise.DifficultyLevel,
                EquipmentRequired: equipment,
                Description: exercise.Description,
                VideoUrl: exercise.VideoUrl,
                DefaultSets: 3,  // Default values since not in entity
                DefaultReps: "8-12",
                DefaultRestSeconds: 60
            );
        }
    }
}

