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

        public ExerciseLibraryService(
            PersonalCoachDbContext context,
            ILogger<ExerciseLibraryService> logger)
        {
            _context = context;
            _logger = logger;
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
                        e.ExerciseName.ToLower().Contains(searchLower) ||
                        e.ExerciseNameVi.ToLower().Contains(searchLower) ||
                        e.Description.ToLower().Contains(searchLower));
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
            var exercises = await _context.ExerciseLibrary
                .Where(e => e.IsActive)
                .Where(e => 
                    e.ExerciseName.ToLower().Contains(searchLower) ||
                    e.ExerciseNameVi.ToLower().Contains(searchLower) ||
                    e.Description.ToLower().Contains(searchLower) ||
                    e.TargetMuscles.ToLower().Contains(searchLower))
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
            // Use RAG search from existing exercise library
            // This is a lightweight operation that doesn't consume AI generation quota
            var query = _context.ExerciseLibrary.Where(e => e.IsActive);

            // Filter by focus area
            if (!string.IsNullOrEmpty(request.FocusArea))
            {
                var focusLower = request.FocusArea.ToLower();
                query = query.Where(e => 
                    e.Category.ToLower().Contains(focusLower) ||
                    e.TargetMuscles.ToLower().Contains(focusLower));
            }

            // Filter by difficulty
            if (!string.IsNullOrEmpty(request.Difficulty))
            {
                query = query.Where(e => e.DifficultyLevel == request.Difficulty);
            }

            // Filter by equipment
            if (request.EquipmentAvailable != null && request.EquipmentAvailable.Any())
            {
                // Simplified: only show bodyweight exercises if equipment list is provided
                query = query.Where(e => e.EquipmentRequired == "[]" || e.EquipmentRequired == "");
            }

            var exercises = await query
                .OrderBy(e => Guid.NewGuid()) // Random order for variety
                .Take(request.Count)
                .ToListAsync();

            _logger.LogInformation(
                "Generated {Count} AI suggestions for {FocusArea} / {Difficulty}",
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

