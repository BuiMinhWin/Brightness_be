using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class WorkoutScheduleExercise
    {
        public Guid Id { get; set; }
        public Guid ScheduleDayId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string ExerciseCategory { get; set; } = string.Empty; // strength, cardio, flexibility
        public string TargetMuscles { get; set; } = string.Empty; // JSON array
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty; // can be range like "8-12"
        public int DurationSeconds { get; set; }
        public int RestSeconds { get; set; }
        public decimal WeightKg { get; set; }
        public int OrderIndex { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int? ActualSets { get; set; }
        public int? ActualReps { get; set; }
        // public decimal? ActualWeightKg { get; set; }  // TODO: Add when implementing workout tracking
        public DateTime CreatedAt { get; set; }
        // public DateTime? UpdatedAt { get; set; }  // TODO: Add when implementing workout tracking
        
        // Navigation property
        public WorkoutScheduleDay ScheduleDay { get; set; } = null!;
    }
}

