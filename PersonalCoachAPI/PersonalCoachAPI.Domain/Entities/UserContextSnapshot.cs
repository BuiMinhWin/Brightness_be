using System;
using Pgvector;

namespace PersonalCoachAPI.Domain.Entities
{
    public class UserContextSnapshot
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public string HealthSummary { get; set; } = string.Empty;
        public string FitnessSummary { get; set; } = string.Empty;
        public string GoalsSummary { get; set; } = string.Empty;
        public string ProgressSummary { get; set; } = string.Empty;
        public Vector? EmbeddingVector { get; set; } // pgvector for context retrieval
        public DateTime CreatedAt { get; set; }
    }
}

