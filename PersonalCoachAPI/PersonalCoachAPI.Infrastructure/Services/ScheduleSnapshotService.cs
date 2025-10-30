using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    /// <summary>
    /// Git-like snapshot service - captures complete state before every change
    /// Enables point-in-time restoration and granular change tracking
    /// </summary>
    public class ScheduleSnapshotService : IScheduleSnapshotService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<ScheduleSnapshotService> _logger;

        public ScheduleSnapshotService(
            PersonalCoachDbContext context,
            ILogger<ScheduleSnapshotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> CreateSnapshotAsync(CreateSnapshotRequest request)
        {
            try
            {
                // Get the complete current state of the schedule
                var schedule = await _context.WorkoutSchedules
                    .Include(s => s.ScheduleDays)
                        .ThenInclude(d => d.Exercises)
                    .FirstOrDefaultAsync(s => s.Id == request.ScheduleId);

                if (schedule == null)
                {
                    _logger.LogWarning("Schedule {ScheduleId} not found for snapshot creation", request.ScheduleId);
                    throw new InvalidOperationException($"Schedule {request.ScheduleId} not found");
                }

                // Get next snapshot number for this schedule
                var lastSnapshotNumber = await _context.ScheduleSnapshots
                    .Where(s => s.ScheduleId == request.ScheduleId)
                    .MaxAsync(s => (int?)s.SnapshotNumber) ?? 0;

                // Serialize complete state
                var stateSnapshot = SerializeScheduleState(schedule);

                // Create snapshot
                var snapshot = new ScheduleSnapshot
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = request.ScheduleId,
                    UserId = request.UserId,
                    ActionType = request.ActionType,
                    Source = request.Source,
                    ScheduleStateSnapshot = stateSnapshot,
                    ChangeDetails = request.ChangeDetails != null 
                        ? JsonSerializer.Serialize(request.ChangeDetails) 
                        : null,
                    ChangeReason = request.ChangeReason,
                    UserNote = request.UserNote,
                    AffectedEntityType = request.AffectedEntityType,
                    AffectedEntityId = request.AffectedEntityId,
                    TokensUsed = request.TokensUsed,
                    SnapshotNumber = lastSnapshotNumber + 1,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.ScheduleSnapshots.AddAsync(snapshot);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Created snapshot #{Number} for schedule {ScheduleId}: {ActionType} ({Source})",
                    snapshot.SnapshotNumber, request.ScheduleId, request.ActionType, request.Source);

                return snapshot.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating snapshot for schedule {ScheduleId}", request.ScheduleId);
                throw;
            }
        }

        public async Task<List<ScheduleSnapshotResponse>> GetScheduleSnapshotsAsync(Guid scheduleId, int limit = 100)
        {
            var snapshots = await _context.ScheduleSnapshots
                .Where(s => s.ScheduleId == scheduleId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(limit)
                .Select(s => new ScheduleSnapshotResponse
                {
                    Id = s.Id,
                    ScheduleId = s.ScheduleId,
                    SnapshotNumber = s.SnapshotNumber,
                    ActionType = s.ActionType,
                    Source = s.Source,
                    ChangeReason = s.ChangeReason,
                    UserNote = s.UserNote,
                    ChangeDetails = s.ChangeDetails,
                    AffectedEntityType = s.AffectedEntityType,
                    AffectedEntityId = s.AffectedEntityId,
                    TokensUsed = s.TokensUsed,
                    CreatedAt = s.CreatedAt,
                    ChangeSummary = GenerateChangeSummary(s)
                })
                .ToListAsync();

            return snapshots;
        }

        public async Task<List<ScheduleSnapshotResponse>> GetUserSnapshotsAsync(Guid userId, int limit = 100)
        {
            var snapshots = await _context.ScheduleSnapshots
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(limit)
                .Select(s => new ScheduleSnapshotResponse
                {
                    Id = s.Id,
                    ScheduleId = s.ScheduleId,
                    SnapshotNumber = s.SnapshotNumber,
                    ActionType = s.ActionType,
                    Source = s.Source,
                    ChangeReason = s.ChangeReason,
                    UserNote = s.UserNote,
                    ChangeDetails = s.ChangeDetails,
                    AffectedEntityType = s.AffectedEntityType,
                    AffectedEntityId = s.AffectedEntityId,
                    TokensUsed = s.TokensUsed,
                    CreatedAt = s.CreatedAt,
                    ChangeSummary = GenerateChangeSummary(s)
                })
                .ToListAsync();

            return snapshots;
        }

        public async Task<UserScheduleAnalytics> GetUserAnalyticsAsync(Guid userId)
        {
            var snapshots = await _context.ScheduleSnapshots
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var analytics = new UserScheduleAnalytics
            {
                UserId = userId,
                TotalSnapshots = snapshots.Count,
                TotalManualChanges = snapshots.Count(s => s.Source == "user_manual"),
                TotalAIRegenerations = snapshots.Count(s => s.Source == "ai_generation"),
                TotalTokensUsed = snapshots.Sum(s => s.TokensUsed),
                ActionTypeDistribution = snapshots
                    .GroupBy(s => s.ActionType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ChangeReasons = snapshots
                    .Where(s => s.ChangeReason != null)
                    .GroupBy(s => s.ChangeReason!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentSnapshots = snapshots
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(20)
                    .Select(s => new ScheduleSnapshotResponse
                    {
                        Id = s.Id,
                        ScheduleId = s.ScheduleId,
                        SnapshotNumber = s.SnapshotNumber,
                        ActionType = s.ActionType,
                        Source = s.Source,
                        ChangeReason = s.ChangeReason,
                        UserNote = s.UserNote,
                        ChangeDetails = s.ChangeDetails,
                        AffectedEntityType = s.AffectedEntityType,
                        AffectedEntityId = s.AffectedEntityId,
                        TokensUsed = s.TokensUsed,
                        CreatedAt = s.CreatedAt,
                        ChangeSummary = GenerateChangeSummary(s)
                    })
                    .ToList(),
                FirstActivityDate = snapshots.Any() ? snapshots.Min(s => s.CreatedAt) : null,
                LastActivityDate = snapshots.Any() ? snapshots.Max(s => s.CreatedAt) : null
            };

            return analytics;
        }

        public async Task<WorkoutScheduleResponse> RevertToSnapshotAsync(Guid snapshotId, Guid userId)
        {
            // Get the snapshot
            var snapshot = await _context.ScheduleSnapshots
                .FirstOrDefaultAsync(s => s.Id == snapshotId && s.UserId == userId);

            if (snapshot == null)
            {
                throw new InvalidOperationException("Snapshot not found or access denied");
            }

            // Deserialize the saved state
            var savedState = JsonSerializer.Deserialize<ScheduleState>(
                snapshot.ScheduleStateSnapshot);

            if (savedState == null)
            {
                throw new InvalidOperationException("Failed to deserialize snapshot state");
            }

            // Create a snapshot of current state before reverting
            await CreateSnapshotAsync(new CreateSnapshotRequest
            {
                ScheduleId = snapshot.ScheduleId,
                UserId = userId,
                ActionType = "reverted_to_snapshot",
                Source = "user_manual",
                ChangeDetails = new
                {
                    revertedToSnapshotId = snapshotId,
                    revertedToSnapshotNumber = snapshot.SnapshotNumber,
                    revertedToDate = snapshot.CreatedAt
                },
                TokensUsed = 0
            });

            // Restore the schedule state
            var restoredSchedule = await RestoreScheduleState(snapshot.ScheduleId, savedState);

            _logger.LogInformation(
                "Reverted schedule {ScheduleId} to snapshot #{Number} ({Date})",
                snapshot.ScheduleId, snapshot.SnapshotNumber, snapshot.CreatedAt);

            return restoredSchedule;
        }

        public async Task<ScheduleSnapshotResponse?> GetSnapshotAsync(Guid snapshotId)
        {
            var snapshot = await _context.ScheduleSnapshots
                .FirstOrDefaultAsync(s => s.Id == snapshotId);

            if (snapshot == null)
                return null;

            return new ScheduleSnapshotResponse
            {
                Id = snapshot.Id,
                ScheduleId = snapshot.ScheduleId,
                SnapshotNumber = snapshot.SnapshotNumber,
                ActionType = snapshot.ActionType,
                Source = snapshot.Source,
                ChangeReason = snapshot.ChangeReason,
                UserNote = snapshot.UserNote,
                ChangeDetails = snapshot.ChangeDetails,
                AffectedEntityType = snapshot.AffectedEntityType,
                AffectedEntityId = snapshot.AffectedEntityId,
                TokensUsed = snapshot.TokensUsed,
                CreatedAt = snapshot.CreatedAt,
                ChangeSummary = GenerateChangeSummary(snapshot)
            };
        }

        // Helper methods

        private string SerializeScheduleState(WorkoutSchedule schedule)
        {
            var state = new ScheduleState
            {
                ScheduleId = schedule.Id,
                UserId = schedule.UserId,
                ScheduleName = schedule.ScheduleName,
                Goal = schedule.Goal,
                DifficultyLevel = schedule.DifficultyLevel,
                DurationDays = schedule.DurationDays,
                Status = schedule.Status,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                Days = schedule.ScheduleDays.Select(d => new ScheduleDayState
                {
                    Id = d.Id,
                    DayNumber = d.DayNumber,
                    Date = d.Date,
                    DayType = d.DayType,
                    FocusArea = d.FocusArea,
                    EstimatedDuration = d.EstimatedDuration,
                    Completed = d.Completed,
                    Exercises = d.Exercises.Select(e => new ExerciseState
                    {
                        Id = e.Id,
                        ExerciseName = e.ExerciseName,
                        Category = e.ExerciseCategory,
                        Sets = e.Sets,
                        Reps = e.Reps,
                        RestSeconds = e.RestSeconds,
                        DurationSeconds = e.DurationSeconds,
                        WeightKg = e.WeightKg,
                        Notes = e.Notes,
                        VideoUrl = e.VideoUrl,
                        Completed = e.Completed,
                        OrderIndex = e.OrderIndex
                    }).ToList()
                }).ToList()
            };

            return JsonSerializer.Serialize(state);
        }

        private async Task<WorkoutScheduleResponse> RestoreScheduleState(Guid scheduleId, ScheduleState savedState)
        {
            // Get current schedule
            var schedule = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                    .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
            {
                throw new InvalidOperationException($"Schedule {scheduleId} not found");
            }

            // Remove all existing days and exercises (cascade delete will handle exercises)
            _context.WorkoutScheduleDays.RemoveRange(schedule.ScheduleDays);

            // Restore schedule properties
            schedule.ScheduleName = savedState.ScheduleName;
            schedule.Goal = savedState.Goal;
            schedule.DifficultyLevel = savedState.DifficultyLevel;
            schedule.DurationDays = savedState.DurationDays;
            schedule.Status = savedState.Status;
            schedule.StartDate = savedState.StartDate;
            schedule.EndDate = savedState.EndDate;
            schedule.UpdatedAt = DateTime.UtcNow;

            // Restore days and exercises
            foreach (var dayState in savedState.Days)
            {
                var day = new WorkoutScheduleDay
                {
                    Id = Guid.NewGuid(), // New ID
                    ScheduleId = scheduleId,
                    DayNumber = dayState.DayNumber,
                    Date = dayState.Date,
                    DayType = dayState.DayType,
                    FocusArea = dayState.FocusArea,
                    EstimatedDuration = dayState.EstimatedDuration,
                    Completed = dayState.Completed,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var exerciseState in dayState.Exercises)
                {
                    day.Exercises.Add(new WorkoutScheduleExercise
                    {
                        Id = Guid.NewGuid(), // New ID
                        ScheduleDayId = day.Id,
                        ExerciseName = exerciseState.ExerciseName,
                        ExerciseCategory = exerciseState.Category,
                        Sets = exerciseState.Sets,
                        Reps = exerciseState.Reps,
                        RestSeconds = exerciseState.RestSeconds,
                        DurationSeconds = exerciseState.DurationSeconds,
                        WeightKg = exerciseState.WeightKg ?? 0,
                        Notes = exerciseState.Notes ?? string.Empty,
                        VideoUrl = exerciseState.VideoUrl ?? string.Empty,
                        Completed = exerciseState.Completed,
                        OrderIndex = exerciseState.OrderIndex,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                schedule.ScheduleDays.Add(day);
            }

            await _context.SaveChangesAsync();

            // Return the restored schedule
            return MapToResponse(schedule);
        }

        private string GenerateChangeSummary(ScheduleSnapshot snapshot)
        {
            try
            {
                if (string.IsNullOrEmpty(snapshot.ChangeDetails))
                {
                    return GetDefaultSummary(snapshot.ActionType, snapshot.Source);
                }

                var details = JsonSerializer.Deserialize<JsonElement>(snapshot.ChangeDetails);

                return snapshot.ActionType switch
                {
                    "exercise_added" => $"Added \"{details.GetProperty("exerciseName").GetString()}\" to Day {details.GetProperty("dayNumber").GetInt32()}",
                    "exercise_removed" => $"Removed \"{details.GetProperty("exerciseName").GetString()}\"",
                    "exercise_modified" => $"Modified \"{details.GetProperty("exerciseName").GetString()}\"",
                    "ai_regenerated" => $"AI Regenerated ({snapshot.ChangeReason ?? "user_request"})",
                    "reverted_to_snapshot" => $"Reverted to snapshot #{details.GetProperty("revertedToSnapshotNumber").GetInt32()}",
                    _ => GetDefaultSummary(snapshot.ActionType, snapshot.Source)
                };
            }
            catch
            {
                return GetDefaultSummary(snapshot.ActionType, snapshot.Source);
            }
        }

        private string GetDefaultSummary(string actionType, string source)
        {
            return actionType switch
            {
                "created" => "Schedule Created",
                "ai_regenerated" => "AI Regenerated",
                "exercise_added" => "Exercise Added",
                "exercise_removed" => "Exercise Removed",
                "exercise_modified" => "Exercise Modified",
                "day_added" => "Day Added",
                "day_removed" => "Day Removed",
                "difficulty_adjusted" => "Difficulty Adjusted",
                "reverted_to_snapshot" => "Reverted to Previous State",
                "schedule_modified" => "Schedule Modified",
                _ => $"{actionType} ({source})"
            };
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
                Days = schedule.ScheduleDays.OrderBy(d => d.DayNumber).Select(d => new ScheduleDayDetail
                {
                    ScheduleDayId = d.Id,
                    DayNumber = d.DayNumber,
                    Date = d.Date,
                    DayType = d.DayType,
                    FocusArea = d.FocusArea,
                    EstimatedDuration = d.EstimatedDuration,
                    Exercises = d.Exercises.OrderBy(e => e.OrderIndex).Select(e => new ExerciseDetail
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

    // Helper classes for state serialization
    public class ScheduleState
    {
        public Guid ScheduleId { get; set; }
        public Guid UserId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ScheduleDayState> Days { get; set; } = new();
    }

    public class ScheduleDayState
    {
        public Guid Id { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string FocusArea { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
        public bool Completed { get; set; }
        public List<ExerciseState> Exercises { get; set; } = new();
    }

    public class ExerciseState
    {
        public Guid Id { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public int DurationSeconds { get; set; }
        public decimal? WeightKg { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }
        public bool Completed { get; set; }
        public int OrderIndex { get; set; }
    }
}

