using PersonalCoachAPI.Domain.Entities;

namespace PersonalCoachAPI.Application.DTOs
{
    // Request DTOs
    public class GenerateScheduleRequest
    {
        public Guid UserId { get; set; }
        public string Goal { get; set; } = string.Empty;
        public int DurationDays { get; set; } = 7;
        public string DifficultyLevel { get; set; } = string.Empty;
        public List<string> AvailableDays { get; set; } = new();
        public List<string> EquipmentAvailable { get; set; } = new();
        public string? AdditionalNotes { get; set; }
        public bool StartFromToday { get; set; } = false;
        public DateTime? StartDate { get; set; }
    }

    public class ModifyDayRequest
    {
        public Guid ScheduleId { get; set; }
        public int DayNumber { get; set; }
        public string? NewFocusArea { get; set; }
        public string? UserRequest { get; set; } // e.g., "Make it easier" or "Add more cardio"
    }

    public class AdjustDifficultyRequest
    {
        public Guid ScheduleId { get; set; }
        public string AdjustmentType { get; set; } = string.Empty; // "increase" or "decrease"
        public int Percentage { get; set; }
        public string? Reason { get; set; }
    }

    public class CompleteExerciseRequest
    {
        public bool Completed { get; set; }
        public int? ActualSets { get; set; }
        public int? ActualReps { get; set; }
    }

    // Response DTOs
    public class WorkoutScheduleResponse
    {
        public Guid ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public List<ScheduleDayDetail> Days { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public bool IsLatest { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    public class ScheduleDayDetail
    {
        public Guid ScheduleDayId { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string FocusArea { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
        public List<ExerciseDetail> Exercises { get; set; } = new();
    }

    public class ExerciseDetail
    {
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }
        public bool Completed { get; set; }
        public int? ActualSets { get; set; }
        public int? ActualReps { get; set; }
    }

    public class ModifiedDayResponse
    {
        public Guid ScheduleId { get; set; }
        public int DayNumber { get; set; }
        public ScheduleDayDetail ModifiedDay { get; set; } = new();
        public string ModificationSummary { get; set; } = string.Empty;
    }

    public class ScheduleAdjustmentResponse
    {
        public Guid ScheduleId { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public List<DayAdjustment> Adjustments { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class DayAdjustment
    {
        public int DayNumber { get; set; }
        public List<string> Changes { get; set; } = new();
    }

    // Feedback DTOs
    public class SubmitWorkoutFeedbackRequest
    {
        public Guid ScheduleId { get; set; }
        public Guid ScheduleDayId { get; set; }
        public int DayNumber { get; set; }
        public string DifficultyRating { get; set; } = string.Empty; // "too_easy", "just_right", "too_hard", "could_not_complete"
        public int? EnergyLevel { get; set; } // 1-5
        public int? SorenessLevel { get; set; } // 1-5
        public bool? TookLongerThanExpected { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public string? Notes { get; set; }
        public List<string>? ContextTags { get; set; } // ["busy_day", "not_enough_sleep", etc.]
    }

    public class RegenerateScheduleRequest
    {
        public Guid CurrentScheduleId { get; set; }
        public int FromDayNumber { get; set; }
        public string Reason { get; set; } = string.Empty; // "too_hard", "too_easy", "busy", "injury", "user_request"
        public string? AdditionalNotes { get; set; }
        public bool PreserveCompleted { get; set; } = true;
        public string? NewDifficultyLevel { get; set; } // Optional: override difficulty
        public List<string>? NewAvailableDays { get; set; } // Optional: change available days
    }

    public class EditWorkoutDayRequest
    {
        public Guid ScheduleDayId { get; set; }
        public string? NewFocusArea { get; set; }
        public List<EditExerciseRequest>? ModifiedExercises { get; set; }
        public List<Guid>? ExercisesToRemove { get; set; }
        public List<AddExerciseRequest>? ExercisesToAdd { get; set; }
    }

    public class EditExerciseRequest
    {
        public Guid ExerciseId { get; set; }
        public int? NewSets { get; set; }
        public string? NewReps { get; set; }
        public int? NewRestSeconds { get; set; }
        public string? NewNotes { get; set; }
    }

    public class AddExerciseRequest
    {
        public string ExerciseName { get; set; } = string.Empty;
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public int OrderIndex { get; set; }
    }

    // Response DTOs
    public class ScheduleHistoryResponse
    {
        public List<ScheduleVersionInfo> Versions { get; set; } = new();
        public ScheduleAnalytics Analytics { get; set; } = new();
    }

    public class ScheduleVersionInfo
    {
        public Guid ScheduleId { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLatest { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int CompletedDays { get; set; }
        public int CompletionPercentage { get; set; }
        public string? RegenerationReason { get; set; }
    }

    public class ScheduleAnalytics
    {
        public int TotalWorkoutsCompleted { get; set; }
        public int TotalWorkoutsPlanned { get; set; }
        public int OverallCompletionRate { get; set; }
        public Dictionary<string, int> CompletionByFocusArea { get; set; } = new();
        public List<string> MostCompletedExercises { get; set; } = new();
        public double AverageDifficulty { get; set; }
        public List<FeedbackSummary> RecentFeedback { get; set; } = new();
    }

    public class FeedbackSummary
    {
        public DateTime Date { get; set; }
        public string DifficultyRating { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    // Conflict Detection DTOs
    public class CheckConflictsRequest
    {
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public int DurationDays { get; set; }
        public List<string> AvailableDays { get; set; } = new();
    }

    public class ScheduleConflictInfo
    {
        public bool HasConflicts { get; set; }
        public List<ConflictingWorkout> ConflictingWorkouts { get; set; } = new();
        public int TotalConflicts { get; set; }
    }

    public class ConflictingWorkout
    {
        public DateTime Date { get; set; }
        public string FocusArea { get; set; } = string.Empty;
        public int ExerciseCount { get; set; }
        public bool IsCompleted { get; set; }
    }
}
