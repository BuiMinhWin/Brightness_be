using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Domain.Entities
{
    public class WorkoutSchedule
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; } // 7 or 14
        public string Goal { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "beginner";
        public string Status { get; set; } = "active"; // active, completed, paused, cancelled, archived
        public string AiGenerationPrompt { get; set; } = string.Empty;
        public bool CreatedByAi { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Versioning and History
        public int Version { get; set; } = 1;
        public Guid? ParentScheduleId { get; set; } // Links to previous version
        public int? RegeneratedFromDay { get; set; } // Which day number triggered regeneration
        public string? RegenerationReason { get; set; } // "too_hard", "too_easy", "busy", "user_request"
        public bool IsLatest { get; set; } = true;
        
        // Navigation properties
        public ICollection<WorkoutScheduleDay> ScheduleDays { get; set; } = new List<WorkoutScheduleDay>();
        public ICollection<WorkoutFeedback> Feedbacks { get; set; } = new List<WorkoutFeedback>();
    }
}

