using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class WorkoutFeedback
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid ScheduleDayId { get; set; }
        public int DayNumber { get; set; }
        
        // Feedback Details
        public string FeedbackType { get; set; } = string.Empty; // "difficulty", "timing", "exercise_specific"
        public string DifficultyRating { get; set; } = string.Empty; // "too_easy", "just_right", "too_hard", "could_not_complete"
        public int? EnergyLevel { get; set; } // 1-5 scale
        public int? SorenessLevel { get; set; } // 1-5 scale
        public string? Notes { get; set; } // Free-form user notes
        
        // Timing Issues
        public bool? TookLongerThanExpected { get; set; }
        public int? ActualDurationMinutes { get; set; }
        
        // Exercise-specific feedback
        public Guid? ExerciseId { get; set; } // If feedback is about specific exercise
        public string? ExerciseFeedback { get; set; } // "too_hard", "wrong_equipment", "caused_pain", "liked_it"
        
        // Context
        public string? ContextTags { get; set; } // JSON array: ["busy_day", "not_enough_sleep", "injury_flare_up"]
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public WorkoutSchedule? Schedule { get; set; }
        public WorkoutScheduleDay? ScheduleDay { get; set; }
    }
}

