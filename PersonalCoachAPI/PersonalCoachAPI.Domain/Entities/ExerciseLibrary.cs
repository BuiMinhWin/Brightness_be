using System;
using Pgvector;

namespace PersonalCoachAPI.Domain.Entities
{
    public class ExerciseLibrary
    {
        public Guid Id { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string ExerciseNameVi { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // strength, cardio, flexibility
        public string DifficultyLevel { get; set; } = string.Empty; // beginner, intermediate, advanced
        public string EquipmentRequired { get; set; } = string.Empty; // JSON array
        public string TargetMuscles { get; set; } = string.Empty; // JSON array
        public string SecondaryMuscles { get; set; } = string.Empty; // JSON array
        public string Description { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty; // JSON array of steps
        public string Benefits { get; set; } = string.Empty; // JSON array
        public string CommonMistakes { get; set; } = string.Empty; // JSON array
        public string Variations { get; set; } = string.Empty; // JSON array
        public string VideoUrl { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal CaloriesPerMinute { get; set; }
        public Vector? EmbeddingVector { get; set; } // pgvector for semantic search
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

