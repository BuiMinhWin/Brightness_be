using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class WorkoutTemplateService : IWorkoutTemplateService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<WorkoutTemplateService> _logger;

        public WorkoutTemplateService(
            PersonalCoachDbContext context,
            ILogger<WorkoutTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<WorkoutTemplateDTO>> GetAllTemplatesAsync(TemplateFilters? filters = null)
        {
            var query = _context.WorkoutTemplates
                .Include(t => t.Exercises)
                .ThenInclude(e => e.Exercise)
                .Where(t => t.IsActive);

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.FocusArea))
                {
                    query = query.Where(t => t.FocusArea == filters.FocusArea);
                }

                if (!string.IsNullOrEmpty(filters.Difficulty))
                {
                    query = query.Where(t => t.Difficulty == filters.Difficulty);
                }
            }

            var templates = await query
                .OrderBy(t => t.FocusArea)
                .ThenBy(t => t.Difficulty)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();

            return templates.Select(MapToDTO).ToList();
        }

        public async Task<WorkoutTemplateDTO?> GetTemplateByIdAsync(Guid id)
        {
            var template = await _context.WorkoutTemplates
                .Include(t => t.Exercises)
                .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(t => t.Id == id);

            return template != null ? MapToDTO(template) : null;
        }

        private static WorkoutTemplateDTO MapToDTO(Domain.Entities.WorkoutTemplate template)
        {
            var exercises = template.Exercises
                .OrderBy(e => e.OrderIndex)
                .Select(e => new TemplateExerciseDTO(
                    ExerciseLibraryId: e.ExerciseLibraryId,
                    ExerciseName: e.Exercise.ExerciseName,
                    ExerciseNameVi: e.Exercise.ExerciseNameVi,
                    OrderIndex: e.OrderIndex,
                    Sets: e.Sets,
                    Reps: e.Reps,
                    RestSeconds: e.RestSeconds,
                    Notes: e.Notes
                ))
                .ToList();

            return new WorkoutTemplateDTO(
                Id: template.Id,
                TemplateName: template.TemplateName,
                FocusArea: template.FocusArea,
                Difficulty: template.Difficulty,
                EstimatedDuration: template.EstimatedDuration,
                Description: template.Description,
                Exercises: exercises
            );
        }
    }
}




