using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IAIScheduleService
    {
        // Generate new schedule (FR-007)
        Task<WorkoutScheduleResponse> GenerateScheduleAsync(GenerateScheduleRequest request);
        
        // Modify specific day without regenerating full schedule
        Task<ModifiedDayResponse> ModifyScheduleDayAsync(ModifyDayRequest request);
        
        // Adjust difficulty based on feedback (FR-009)
        Task<ScheduleAdjustmentResponse> AdjustScheduleDifficultyAsync(AdjustDifficultyRequest request);
        
        // Get schedule details
        Task<WorkoutScheduleResponse> GetScheduleAsync(Guid scheduleId);
    }

    public class GenerateScheduleRequest
    {
        public Guid UserId { get; set; }
        public int DurationDays { get; set; } // 7 or 14
        public DateTime StartDate { get; set; }
        public bool IncludeInBodyData { get; set; } = true;
        public bool SyncToCalendar { get; set; } = false;
        public string? CustomGoal { get; set; }
    }

    public class WorkoutScheduleResponse
    {
        public Guid ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public string Goal { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public string AiReasoning { get; set; } = string.Empty;
        public List<ScheduleDayDetail> Days { get; set; } = new();
        public ScheduleSummary Summary { get; set; } = new();
    }

    public class ScheduleDayDetail
    {
        public Guid DayId { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string FocusArea { get; set; } = string.Empty;
        public List<ExerciseDetail> Exercises { get; set; } = new();
        public int EstimatedDurationMinutes { get; set; }
        public int EstimatedCalories { get; set; }
    }

    public class ExerciseDetail
    {
        public Guid ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameVi { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public decimal? WeightKg { get; set; }
        public string TargetMuscles { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
    }

    public class ScheduleSummary
    {
        public int TotalWorkoutDays { get; set; }
        public int TotalRestDays { get; set; }
        public int EstimatedWeeklyCalories { get; set; }
        public List<string> FocusAreas { get; set; } = new();
        public string RecommendedNutrition { get; set; } = string.Empty;
    }

    public class ModifyDayRequest
    {
        public Guid ScheduleId { get; set; }
        public int DayNumber { get; set; }
        public string UserRequest { get; set; } = string.Empty; // "Change chest day to back day"
        public bool KeepDifficulty { get; set; } = true;
    }

    public class ModifiedDayResponse
    {
        public ScheduleDayDetail UpdatedDay { get; set; } = new();
        public string AiExplanation { get; set; } = string.Empty;
        public List<int> AffectedDays { get; set; } = new(); // Other days that might need adjustment
    }

    public class AdjustDifficultyRequest
    {
        public Guid ScheduleId { get; set; }
        public decimal CompletionRate { get; set; } // 0.0 to 1.0
        public decimal? AverageRating { get; set; } // 1-5
        public string? UserFeedback { get; set; }
    }

    public class ScheduleAdjustmentResponse
    {
        public Guid ScheduleId { get; set; }
        public string OldDifficulty { get; set; } = string.Empty;
        public string NewDifficulty { get; set; } = string.Empty;
        public decimal DifficultyChangePercentage { get; set; }
        public string AiReasoning { get; set; } = string.Empty;
        public List<string> ChangesSummary { get; set; } = new();
    }
}

