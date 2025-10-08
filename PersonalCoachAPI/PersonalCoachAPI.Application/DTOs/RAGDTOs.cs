namespace PersonalCoachAPI.Application.DTOs
{
    // RAG Search Results
    public class ExerciseSearchResult
    {
        public Guid Id { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string ExerciseNameVi { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public List<string> TargetMuscles { get; set; } = new();
        public List<string> EquipmentRequired { get; set; } = new();
        public string VideoUrl { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
    }

    public class HealthKnowledgeResult
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ContentVi { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public double SimilarityScore { get; set; }
    }

    public class UserContextResult
    {
        public Guid UserId { get; set; }
        public string HealthSummary { get; set; } = string.Empty;
        public string FitnessSummary { get; set; } = string.Empty;
        public string GoalsSummary { get; set; } = string.Empty;
        public string ProgressSummary { get; set; } = string.Empty;
        public DateTime SnapshotDate { get; set; }
        public double SimilarityScore { get; set; }
    }
}
