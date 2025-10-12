using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class ScheduleEditHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid? ScheduleDayId { get; set; }
        
        // Edit Details
        public string EditType { get; set; } = string.Empty; // "day_modified", "exercise_changed", "regenerated", "difficulty_adjusted"
        public string EditDescription { get; set; } = string.Empty;
        public string? BeforeValue { get; set; } // JSON of previous state
        public string? AfterValue { get; set; } // JSON of new state
        
        // Metadata
        public string EditSource { get; set; } = "user"; // "user", "ai_adaptation", "feedback_based"
        public DateTime CreatedAt { get; set; }
    }
}

