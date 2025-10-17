namespace ChatService.Application.DTOs
{
    public enum IntentType
    {
        Normal,
        CreateSchedule,
        UpdatePreference,
        QuerySchedule,
        Unsafe
    }

    public record IntentDetectionResult
    {
        public IntentType Type { get; init; }
        public bool IsSafe { get; init; }
        public Dictionary<string, string> ExtractedParams { get; init; } = new();
    }
}

