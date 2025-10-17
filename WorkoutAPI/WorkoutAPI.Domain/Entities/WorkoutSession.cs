using System;
using System.Collections.Generic;

namespace WorkoutAPI.Domain.Entities
{
    public class WorkoutSession
    {
        public Guid Id { get; set; }
        public Guid WorkoutPlanId { get; set; }
        public DateOnly SessionDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public bool Completed { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<SessionExercise> Exercises { get; set; } = new List<SessionExercise>();
    }
}
