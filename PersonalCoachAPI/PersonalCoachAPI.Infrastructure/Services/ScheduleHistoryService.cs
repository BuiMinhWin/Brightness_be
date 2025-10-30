using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public class ScheduleHistoryService : IScheduleHistoryService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<ScheduleHistoryService> _logger;

        public ScheduleHistoryService(
            PersonalCoachDbContext context,
            ILogger<ScheduleHistoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RecordScheduleActionAsync(
            Guid scheduleId,
            Guid userId,
            string actionType,
            int version,
            string? changeReason = null,
            string? userNotes = null,
            int? fromDayNumber = null,
            string? previousDifficulty = null,
            string? newDifficulty = null,
            int tokensUsed = 0)
        {
            try
            {
                var schedule = await _context.WorkoutSchedules
                    .Include(s => s.ScheduleDays)
                        .ThenInclude(d => d.Exercises)
                    .FirstOrDefaultAsync(s => s.Id == scheduleId);

                if (schedule == null)
                {
                    _logger.LogWarning("Schedule {ScheduleId} not found for history recording", scheduleId);
                    return;
                }

                var completedDays = schedule.ScheduleDays.Count(d => d.Exercises.All(e => e.Completed));
                var totalDays = schedule.ScheduleDays.Count;
                var completionRate = totalDays > 0 ? (decimal)completedDays / totalDays * 100 : 0;

                var historyEntry = new ScheduleHistory
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = scheduleId,
                    UserId = userId,
                    Version = version,
                    ActionType = actionType,
                    ChangeReason = changeReason,
                    UserNotes = userNotes,
                    FromDayNumber = fromDayNumber,
                    PreviousDifficulty = previousDifficulty,
                    NewDifficulty = newDifficulty ?? schedule.DifficultyLevel,
                    TotalDays = totalDays,
                    CompletedDays = completedDays,
                    CompletionRate = completionRate,
                    TokensUsed = tokensUsed,
                    CreatedAt = DateTime.UtcNow,
                    ConfigurationSnapshot = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        goal = schedule.Goal,
                        difficulty = schedule.DifficultyLevel,
                        duration = schedule.DurationDays,
                        startDate = schedule.StartDate,
                        endDate = schedule.EndDate
                    })
                };

                await _context.ScheduleHistories.AddAsync(historyEntry);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Recorded {ActionType} for schedule {ScheduleId} version {Version}",
                    actionType, scheduleId, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording schedule history for {ScheduleId}", scheduleId);
                // Don't throw - history recording shouldn't break the main flow
            }
        }

        public async Task<List<ScheduleHistoryEntry>> GetUserScheduleHistoryAsync(Guid userId, int limit = 50)
        {
            var history = await _context.ScheduleHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(limit)
                .Select(h => new ScheduleHistoryEntry
                {
                    Id = h.Id,
                    ScheduleId = h.ScheduleId,
                    Version = h.Version,
                    ActionType = h.ActionType,
                    ChangeReason = h.ChangeReason,
                    UserNotes = h.UserNotes,
                    FromDayNumber = h.FromDayNumber,
                    PreviousDifficulty = h.PreviousDifficulty,
                    NewDifficulty = h.NewDifficulty,
                    TotalDays = h.TotalDays,
                    CompletedDays = h.CompletedDays,
                    CompletionRate = h.CompletionRate,
                    TokensUsed = h.TokensUsed,
                    CreatedAt = h.CreatedAt,
                    ConfigurationSnapshot = h.ConfigurationSnapshot
                })
                .ToListAsync();

            return history;
        }

        public async Task<List<ScheduleHistoryEntry>> GetScheduleVersionHistoryAsync(Guid scheduleId)
        {
            var history = await _context.ScheduleHistories
                .Where(h => h.ScheduleId == scheduleId)
                .OrderByDescending(h => h.Version)
                .Select(h => new ScheduleHistoryEntry
                {
                    Id = h.Id,
                    ScheduleId = h.ScheduleId,
                    Version = h.Version,
                    ActionType = h.ActionType,
                    ChangeReason = h.ChangeReason,
                    UserNotes = h.UserNotes,
                    FromDayNumber = h.FromDayNumber,
                    PreviousDifficulty = h.PreviousDifficulty,
                    NewDifficulty = h.NewDifficulty,
                    TotalDays = h.TotalDays,
                    CompletedDays = h.CompletedDays,
                    CompletionRate = h.CompletionRate,
                    TokensUsed = h.TokensUsed,
                    CreatedAt = h.CreatedAt,
                    ConfigurationSnapshot = h.ConfigurationSnapshot
                })
                .ToListAsync();

            return history;
        }

        public async Task<UserScheduleAnalytics> GetScheduleAnalyticsAsync(Guid userId)
        {
            var allHistory = await _context.ScheduleHistories
                .Where(h => h.UserId == userId)
                .ToListAsync();

            var schedules = await _context.WorkoutSchedules
                .Where(s => s.UserId == userId)
                .Include(s => s.ScheduleDays)
                    .ThenInclude(d => d.Exercises)
                .ToListAsync();

            var analytics = new UserScheduleAnalytics
            {
                UserId = userId,
                TotalSchedulesCreated = allHistory.Count(h => h.ActionType == "created"),
                TotalRegenerations = allHistory.Count(h => h.ActionType == "regenerated"),
                AverageCompletionRate = allHistory.Any() 
                    ? allHistory.Average(h => h.CompletionRate) 
                    : 0,
                TotalWorkoutsCompleted = allHistory.Sum(h => h.CompletedDays),
                TotalTokensUsed = allHistory.Sum(h => h.TokensUsed),
                RegenerationReasons = allHistory
                    .Where(h => h.ChangeReason != null)
                    .GroupBy(h => h.ChangeReason!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                DifficultyDistribution = allHistory
                    .Where(h => h.NewDifficulty != null)
                    .GroupBy(h => h.NewDifficulty!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentVersions = allHistory
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(10)
                    .Join(schedules, h => h.ScheduleId, s => s.Id, (h, s) => new ScheduleVersionDetail
                    {
                        ScheduleId = h.ScheduleId,
                        Version = h.Version,
                        Goal = s.Goal,
                        Difficulty = h.NewDifficulty ?? s.DifficultyLevel,
                        CompletionRate = h.CompletionRate,
                        CreatedAt = h.CreatedAt,
                        ActionType = h.ActionType
                    })
                    .ToList(),
                FirstScheduleDate = allHistory.Any() 
                    ? allHistory.Min(h => h.CreatedAt) 
                    : null,
                LastActivityDate = allHistory.Any() 
                    ? allHistory.Max(h => h.CreatedAt) 
                    : null
            };

            return analytics;
        }

        public async Task<WorkoutScheduleResponse> RevertToVersionAsync(Guid scheduleId, Guid userId)
        {
            // Find the schedule to revert to
            var targetSchedule = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                    .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.UserId == userId);

            if (targetSchedule == null)
            {
                throw new InvalidOperationException("Schedule not found or access denied");
            }

            // Mark current latest as not latest
            var currentLatest = await _context.WorkoutSchedules
                .Where(s => s.UserId == userId && s.IsLatest)
                .ToListAsync();

            foreach (var schedule in currentLatest)
            {
                schedule.IsLatest = false;
            }

            // Mark target as latest and create new version
            targetSchedule.IsLatest = true;
            targetSchedule.Version++;
            targetSchedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Record this revert action
            await RecordScheduleActionAsync(
                targetSchedule.Id,
                userId,
                "reverted",
                targetSchedule.Version,
                changeReason: "reverted_to_previous_version",
                tokensUsed: 0);

            _logger.LogInformation(
                "Reverted user {UserId} to schedule {ScheduleId} version {Version}",
                userId, scheduleId, targetSchedule.Version);

            // Return the schedule response
            return MapToResponse(targetSchedule);
        }

        private WorkoutScheduleResponse MapToResponse(WorkoutSchedule schedule)
        {
            return new WorkoutScheduleResponse
            {
                ScheduleId = schedule.Id,
                ScheduleName = schedule.ScheduleName,
                Goal = schedule.Goal,
                DifficultyLevel = schedule.DifficultyLevel,
                DurationDays = schedule.DurationDays,
                Days = schedule.ScheduleDays.Select(d => new ScheduleDayDetail
                {
                    ScheduleDayId = d.Id,
                    DayNumber = d.DayNumber,
                    Date = d.Date,
                    DayType = d.DayType,
                    FocusArea = d.FocusArea,
                    EstimatedDuration = d.EstimatedDuration,
                    Exercises = d.Exercises.Select(e => new ExerciseDetail
                    {
                        ExerciseId = e.Id,
                        ExerciseName = e.ExerciseName,
                        Category = e.ExerciseCategory,
                        Sets = e.Sets,
                        Reps = e.Reps,
                        RestSeconds = e.RestSeconds,
                        Notes = e.Notes,
                        VideoUrl = e.VideoUrl
                    }).ToList()
                }).ToList(),
                CreatedAt = schedule.CreatedAt
            };
        }
    }
}

