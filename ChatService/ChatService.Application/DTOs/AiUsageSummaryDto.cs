using System.Collections.Generic;

namespace ChatService.Application.DTOs;

public record AiUsageSummaryDto
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public long TotalTokens { get; init; }
    public decimal TotalCost { get; init; }
    public string Currency { get; init; } = "VND";
    public List<AiUsageDailyDto> Daily { get; init; } = new();
}
