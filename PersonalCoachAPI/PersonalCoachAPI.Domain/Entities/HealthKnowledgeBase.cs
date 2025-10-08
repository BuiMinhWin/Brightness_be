using System;
using Pgvector;

namespace PersonalCoachAPI.Domain.Entities
{
    public class HealthKnowledgeBase
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty; // nutrition, medical, fitness, recovery
        public string Topic { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ContentVi { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public decimal ReliabilityScore { get; set; } = 1.0m;
        public string Tags { get; set; } = string.Empty; // JSON array
        public Vector? EmbeddingVector { get; set; } // pgvector for semantic search
        public DateTime CreatedAt { get; set; }
    }
}

