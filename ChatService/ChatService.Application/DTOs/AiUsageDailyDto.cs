namespace ChatService.Application.DTOs;

public record AiUsageDailyDto
{
    public DateOnly Date { get; init; }
    public long Tokens { get; init; }
    public decimal Cost { get; init; }
}
