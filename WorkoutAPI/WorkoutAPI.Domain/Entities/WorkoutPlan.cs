using System;
using System.Collections.Generic;

namespace WorkoutAPI.Domain.Entities
{
    public class WorkoutPlan
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<WorkoutSession> Sessions { get; set; } = new List<WorkoutSession>();
    }
}
