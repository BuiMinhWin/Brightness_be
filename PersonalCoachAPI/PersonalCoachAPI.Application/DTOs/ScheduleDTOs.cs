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
    }

    public class ScheduleDayDetail
    {
        public int DayNumber { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string FocusArea { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
        public List<ExerciseDetail> Exercises { get; set; } = new();
    }

    public class ExerciseDetail
    {
        public string ExerciseName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }
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
}
