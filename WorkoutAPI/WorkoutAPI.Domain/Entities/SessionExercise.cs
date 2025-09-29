using System;

namespace WorkoutAPI.Domain.Entities
{
    public class SessionExercise
    {
        public Guid Id { get; set; }
        public Guid WorkoutSessionId { get; set; }
        public string ExerciseName { get; set; } = null!;
        public int OrderNo { get; set; }
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? DistanceMeters { get; set; }
        public int? DurationSeconds { get; set; }
        public int? RestSeconds { get; set; }
    }
}
