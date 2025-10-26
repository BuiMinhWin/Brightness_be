using System;

namespace ChatService.Domain.Entities;

public class AiUsageLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public string? UserName { get; set; }
    public string? Operation { get; set; }
    public string? Model { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
