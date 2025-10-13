namespace ChatService.Application.DTOs
{
    public record MessageLimitsDTO
    (
        int RemainingDaily,
        int DailyLimit,
        int RemainingMonthly,
        int MonthlyLimit,
        bool IsLimitReached
    );
}

