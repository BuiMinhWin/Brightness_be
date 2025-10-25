namespace ChatService.Application.DTOs
{
    public record ActionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public object? Data { get; init; }
        public string? ErrorCode { get; init; }
        public int? CreditsUsed { get; init; }
        public int? RemainingCredits { get; init; }
    }
}


