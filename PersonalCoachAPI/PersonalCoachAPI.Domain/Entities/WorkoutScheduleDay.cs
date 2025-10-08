using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Domain.Entities
{
    public class WorkoutScheduleDay
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public int DayNumber { get; set; } // 1-14
        public DateTime Date { get; set; }
        public string DayType { get; set; } = "workout"; // workout, rest, active_recovery
        public string FocusArea { get; set; } = string.Empty; // upper_body, lower_body, cardio, full_body
        public bool Completed { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string UserNotes { get; set; } = string.Empty;
        public bool AiModified { get; set; }
        public int ModificationCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public WorkoutSchedule Schedule { get; set; } = null!;
        public ICollection<WorkoutScheduleExercise> Exercises { get; set; } = new List<WorkoutScheduleExercise>();
    }
}

