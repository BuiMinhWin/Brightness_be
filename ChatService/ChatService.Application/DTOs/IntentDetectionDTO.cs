namespace ChatService.Application.DTOs
{
    public enum IntentType
    {
        Normal,
        // Create operations
        CreateSchedule,
        CreateWorkout,
        // Read operations  
        QuerySchedule,
        QueryProfile,
        QueryHealthData,
        QueryProgress,
        // Update operations
        UpdatePreference,
        UpdateProfile,
        UpdateSchedule,
        // Delete operations
        DeleteSchedule,
        DeleteHealthData,
        // Analysis operations
        AnalyzeProgress,
        AnalyzeNutrition,
        GetRecommendations,
        // Unsafe
        Unsafe
    }

    public enum RiskLevel
    {
        Safe,      // Read operations
        Low,       // Create operations
        Medium,    // Update operations  
        High       // Delete operations
    }

    public record IntentDetectionResult
    {
        public IntentType Type { get; init; }
        public bool IsSafe { get; init; }
        public RiskLevel Risk { get; init; }
        public bool RequiresConfirmation { get; init; }
        public Dictionary<string, string> ExtractedParams { get; init; } = new();
        public string? SuggestedAction { get; init; }
    }
}

