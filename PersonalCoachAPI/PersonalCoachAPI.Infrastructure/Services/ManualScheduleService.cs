using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class ManualScheduleService : IManualScheduleService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<ManualScheduleService> _logger;

        public ManualScheduleService(
            PersonalCoachDbContext context,
            ILogger<ManualScheduleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WorkoutScheduleResponse> CreateManualScheduleAsync(ManualScheduleRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating manual schedule for user {UserId}", request.UserId);

                // Archive all existing active schedules for this user
                var existingSchedules = await _context.WorkoutSchedules
                    .Where(s => s.UserId == request.UserId && s.IsLatest)
                    .ToListAsync();

                if (existingSchedules.Any())
                {
                    _logger.LogInformation("Archiving {Count} existing schedules for user {UserId}", 
                        existingSchedules.Count, request.UserId);
                    
                    foreach (var existingSchedule in existingSchedules)
                    {
                        existingSchedule.IsLatest = false;
                        existingSchedule.Status = "archived";
                        existingSchedule.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Get the latest version number
                var latestVersion = await _context.WorkoutSchedules
                    .Where(s => s.UserId == request.UserId)
                    .MaxAsync(s => (int?)s.Version) ?? 0;

                // Create new empty schedule
                var schedule = new WorkoutSchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    ScheduleName = request.ScheduleName,
                    Goal = request.Goal,
                    DifficultyLevel = request.DifficultyLevel,
                    DurationDays = request.DurationDays,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate ?? request.StartDate.AddDays(request.DurationDays),
                    Status = "active",
                    IsLatest = true,
                    Version = latestVersion + 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ScheduleDays = new List<WorkoutScheduleDay>()
                };

                _context.WorkoutSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created manual schedule {ScheduleId}", schedule.Id);

                return new WorkoutScheduleResponse
                {
                    ScheduleId = schedule.Id,
                    ScheduleName = schedule.ScheduleName,
                    Goal = schedule.Goal,
                    DifficultyLevel = schedule.DifficultyLevel,
                    DurationDays = schedule.DurationDays,
                    Days = new List<ScheduleDayDetail>(),
                    CreatedAt = schedule.CreatedAt,
                    IsLatest = schedule.IsLatest,
                    Status = schedule.Status,
                    Version = schedule.Version
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual schedule");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkoutDayResponse> AddWorkoutDayFromTemplateAsync(AddDayFromTemplateRequest request)
        {
            _logger.LogInformation("Adding day from template {TemplateId} to schedule {ScheduleId}", 
                request.TemplateId, request.ScheduleId);

            var schedule = await _context.WorkoutSchedules.FindAsync(request.ScheduleId);
            if (schedule == null)
            {
                throw new ArgumentException("Schedule not found");
            }

            var template = await _context.WorkoutTemplates
                .Include(t => t.Exercises)
                .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId);

            if (template == null)
            {
                throw new ArgumentException("Template not found");
            }

            var scheduleDay = new WorkoutScheduleDay
            {
                Id = Guid.NewGuid(),
                ScheduleId = request.ScheduleId,
                DayNumber = request.DayNumber,
                Date = request.Date,
                DayType = "workout",
                FocusArea = template.FocusArea,
                Completed = false,
                CreatedAt = DateTime.UtcNow,
                Exercises = new List<WorkoutScheduleExercise>()
            };

            foreach (var templateExercise in template.Exercises.OrderBy(e => e.OrderIndex))
            {
                var scheduleExercise = new WorkoutScheduleExercise
                {
                    Id = Guid.NewGuid(),
                    ScheduleDayId = scheduleDay.Id,
                    ExerciseName = templateExercise.Exercise.ExerciseNameVi,
                    ExerciseCategory = templateExercise.Exercise.Category,
                    TargetMuscles = templateExercise.Exercise.TargetMuscles,
                    Sets = templateExercise.Sets,
                    Reps = templateExercise.Reps,
                    RestSeconds = templateExercise.RestSeconds,
                    OrderIndex = templateExercise.OrderIndex,
                    Completed = false,
                    VideoUrl = templateExercise.Exercise.VideoUrl,
                    CreatedAt = DateTime.UtcNow
                };

                scheduleDay.Exercises.Add(scheduleExercise);
            }

            _context.WorkoutScheduleDays.Add(scheduleDay);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully added day {DayNumber} from template to schedule {ScheduleId}", 
                request.DayNumber, request.ScheduleId);

            return MapToWorkoutDayResponse(scheduleDay);
        }

        public async Task<WorkoutDayResponse> AddCustomWorkoutDayAsync(AddCustomDayRequest request)
        {
            _logger.LogInformation("Adding custom day {DayNumber} to schedule {ScheduleId}", 
                request.DayNumber, request.ScheduleId);

            var schedule = await _context.WorkoutSchedules.FindAsync(request.ScheduleId);
            if (schedule == null)
            {
                throw new ArgumentException("Schedule not found");
            }

            var scheduleDay = new WorkoutScheduleDay
            {
                Id = Guid.NewGuid(),
                ScheduleId = request.ScheduleId,
                DayNumber = request.DayNumber,
                Date = request.Date,
                DayType = request.DayType,
                FocusArea = request.FocusArea,
                Completed = false,
                CreatedAt = DateTime.UtcNow,
                Exercises = new List<WorkoutScheduleExercise>()
            };

            foreach (var customExercise in request.Exercises)
            {
                var scheduleExercise = new WorkoutScheduleExercise
                {
                    Id = Guid.NewGuid(),
                    ScheduleDayId = scheduleDay.Id,
                    ExerciseName = customExercise.ExerciseName,
                    ExerciseCategory = customExercise.Category,
                    Sets = customExercise.Sets,
                    Reps = customExercise.Reps,
                    RestSeconds = customExercise.RestSeconds,
                    OrderIndex = customExercise.OrderIndex,
                    Completed = false,
                    Notes = customExercise.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // If exercise is from library, get additional details
                if (customExercise.ExerciseLibraryId.HasValue)
                {
                    var libraryExercise = await _context.ExerciseLibrary
                        .FindAsync(customExercise.ExerciseLibraryId.Value);
                    
                    if (libraryExercise != null)
                    {
                        scheduleExercise.ExerciseName = libraryExercise.ExerciseNameVi;
                        scheduleExercise.TargetMuscles = libraryExercise.TargetMuscles;
                        scheduleExercise.VideoUrl = libraryExercise.VideoUrl;
                    }
                }

                scheduleDay.Exercises.Add(scheduleExercise);
            }

            _context.WorkoutScheduleDays.Add(scheduleDay);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully added custom day {DayNumber} to schedule {ScheduleId}", 
                request.DayNumber, request.ScheduleId);

            return MapToWorkoutDayResponse(scheduleDay);
        }

        public async Task<WorkoutDayResponse> UpdateWorkoutDayAsync(Guid scheduleId, Guid dayId, UpdateWorkoutDayRequest request)
        {
            _logger.LogInformation("Updating day {DayId} in schedule {ScheduleId}", dayId, scheduleId);

            var day = await _context.WorkoutScheduleDays
                .Include(d => d.Exercises)
                .FirstOrDefaultAsync(d => d.Id == dayId && d.ScheduleId == scheduleId);

            if (day == null)
            {
                throw new ArgumentException("Workout day not found");
            }

            // Update day properties if provided
            if (request.DayType != null)
                day.DayType = request.DayType;
            
            if (request.FocusArea != null)
                day.FocusArea = request.FocusArea;

            // If exercises are provided, replace them
            if (request.Exercises != null)
            {
                // Remove old exercises
                _context.WorkoutScheduleExercises.RemoveRange(day.Exercises);

                // Add new exercises
                day.Exercises = new List<WorkoutScheduleExercise>();
                foreach (var customExercise in request.Exercises)
                {
                    var scheduleExercise = new WorkoutScheduleExercise
                    {
                        Id = Guid.NewGuid(),
                        ScheduleDayId = day.Id,
                        ExerciseName = customExercise.ExerciseName,
                        ExerciseCategory = customExercise.Category,
                        Sets = customExercise.Sets,
                        Reps = customExercise.Reps,
                        RestSeconds = customExercise.RestSeconds,
                        OrderIndex = customExercise.OrderIndex,
                        Completed = false,
                        Notes = customExercise.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    // If exercise is from library, get additional details
                    if (customExercise.ExerciseLibraryId.HasValue)
                    {
                        var libraryExercise = await _context.ExerciseLibrary
                            .FindAsync(customExercise.ExerciseLibraryId.Value);
                        
                        if (libraryExercise != null)
                        {
                            scheduleExercise.ExerciseName = libraryExercise.ExerciseNameVi;
                            scheduleExercise.TargetMuscles = libraryExercise.TargetMuscles;
                            scheduleExercise.VideoUrl = libraryExercise.VideoUrl;
                        }
                    }

                    day.Exercises.Add(scheduleExercise);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated day {DayId}", dayId);

            return MapToWorkoutDayResponse(day);
        }

        public async Task DeleteWorkoutDayAsync(Guid scheduleId, Guid dayId)
        {
            _logger.LogInformation("Deleting day {DayId} from schedule {ScheduleId}", dayId, scheduleId);

            var day = await _context.WorkoutScheduleDays
                .FirstOrDefaultAsync(d => d.Id == dayId && d.ScheduleId == scheduleId);

            if (day == null)
            {
                throw new ArgumentException("Workout day not found");
            }

            _context.WorkoutScheduleDays.Remove(day);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted day {DayId}", dayId);
        }

        private static WorkoutDayResponse MapToWorkoutDayResponse(WorkoutScheduleDay day)
        {
            var exercises = day.Exercises
                .OrderBy(e => e.OrderIndex)
                .Select(e => new ExerciseDetailDTO(
                    ExerciseId: e.Id,
                    ExerciseName: e.ExerciseName,
                    Category: e.ExerciseCategory,
                    Sets: e.Sets,
                    Reps: e.Reps,
                    RestSeconds: e.RestSeconds,
                    OrderIndex: e.OrderIndex,
                    Notes: e.Notes ?? string.Empty
                ))
                .ToList();

            return new WorkoutDayResponse(
                DayId: day.Id,
                ScheduleId: day.ScheduleId,
                DayNumber: day.DayNumber,
                Date: day.Date,
                DayType: day.DayType,
                FocusArea: day.FocusArea,
                EstimatedDuration: day.Exercises.Sum(e => (e.Sets * 45) + e.RestSeconds) / 60, // Rough estimate
                Exercises: exercises
            );
        }
    }
}

