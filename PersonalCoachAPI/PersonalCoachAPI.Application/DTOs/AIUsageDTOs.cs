using System;
using System.Collections.Generic;
using PersonalCoachAPI.Domain.Entities;

namespace PersonalCoachAPI.Application.DTOs
{
    // Legacy DTO - kept for backward compatibility
    public record AIUsageLimitsDTO(
        int RemainingGenerations,
        int MonthlyLimit,
        bool CanGenerate,
        int UsedThisMonth,
        int DaysUntilReset,
        string SubscriptionTier
    );

    // New Credit-based DTO
    public record AICreditBalanceDTO(
        int RemainingCredits,
        int MonthlyCredits,
        bool CanAffordOperation,
        int UsedCreditsThisMonth,
        int DaysUntilReset,
        string SubscriptionTier,
        Dictionary<string, int> CreditCostBreakdown
    );

    public record TrackGenerationRequest(
        Guid UserId,
        string GenerationType,
        Guid? RelatedScheduleId
    );

    public record CreditHistoryItemDTO(
        Guid Id,
        string OperationType,
        int CreditCost,
        int RemainingCreditsAfter,
        DateTime CreatedAt,
        Guid? RelatedScheduleId
    );

    public record CanAffordRequest(
        Guid UserId,
        AIOperationType OperationType
    );

    public record DeductCreditRequest(
        Guid UserId,
        AIOperationType OperationType
    );
}




