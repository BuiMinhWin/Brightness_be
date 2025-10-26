using System;

namespace PersonalCoachAPI.Application.DTOs
{
    public record AIUsageLimitsDTO(
        int RemainingGenerations,
        int MonthlyLimit,
        bool CanGenerate,
        int UsedThisMonth,
        int DaysUntilReset,
        string SubscriptionTier
    );

    public record TrackGenerationRequest(
        Guid UserId,
        string GenerationType,
        Guid? RelatedScheduleId
    );
}




