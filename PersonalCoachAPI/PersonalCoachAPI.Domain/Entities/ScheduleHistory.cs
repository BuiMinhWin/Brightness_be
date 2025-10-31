using System;

namespace PersonalCoachAPI.Domain.Entities
{
    /// <summary>
    /// Tracks schedule history with version tracking and analytics
    /// </summary>
    public class ScheduleHistory
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid UserId { get; set; }
        public int Version { get; set; }
        public string ActionType { get; set; } = string.Empty; // "created", "regenerated", "reverted", "modified"
        public string? ChangeReason { get; set; } // Why the change was made
        public string? UserNotes { get; set; } // User-provided notes
        public int? FromDayNumber { get; set; } // For regenerations from a specific day
        public string? PreviousDifficulty { get; set; }
        public string? NewDifficulty { get; set; }
        public int TotalDays { get; set; }
        public int CompletedDays { get; set; }
        public decimal CompletionRate { get; set; }
        public int TokensUsed { get; set; } // AI tokens consumed
        public DateTime CreatedAt { get; set; }
        public string? ConfigurationSnapshot { get; set; } // JSON snapshot of schedule config
    }

    /// <summary>
    /// Git-like snapshot system - captures complete state before every change
    /// Allows granular tracking and point-in-time restoration
    /// </summary>
    public class ScheduleSnapshot
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid UserId { get; set; }
        
        // What happened (granular action types)
        public string ActionType { get; set; } = string.Empty; 
        // "created", "ai_regenerated", "exercise_added", "exercise_removed", 
        // "exercise_modified", "day_added", "day_removed", "difficulty_adjusted",
        // "reverted_to_snapshot", "schedule_modified"
        
        // Who/What triggered the change
        public string Source { get; set; } = string.Empty; 
        // "user_manual", "ai_generation", "chat_agent", "system"
        
        // Complete state snapshot (JSONB) - full schedule at this point in time
        public string ScheduleStateSnapshot { get; set; } = string.Empty; 
        // JSON: Complete WorkoutSchedule with all days and exercises
        
        // Delta information - what specifically changed (JSONB)
        public string? ChangeDetails { get; set; }
        // JSON: { "action": "added", "entityType": "exercise", "entityId": "...", 
        //         "dayNumber": 3, "exerciseName": "Bench Press", ... }
        
        // Context and metadata
        public string? ChangeReason { get; set; } // "too_hard", "too_easy", "busy", "injury", "user_request"
        public string? UserNote { get; set; }
        public int TokensUsed { get; set; } // AI tokens consumed (0 for manual changes)
        
        // Reference to affected entity (for filtering/querying)
        public string? AffectedEntityType { get; set; } // "exercise", "day", "schedule"
        public Guid? AffectedEntityId { get; set; }
        
        // Snapshot metadata
        public int SnapshotNumber { get; set; } // Sequential number for this schedule
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public WorkoutSchedule Schedule { get; set; } = null!;
    }
}

