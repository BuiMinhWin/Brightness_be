using System;

namespace BillingAPI.Application.DTOs
{
    public record AICreditBalanceDTO(
        Guid UserId,
        string ServiceName,
        int RemainingTokens,
        int MonthlyTokenLimit,
        int UsedTokens,
        string SubscriptionTier,
        int DaysUntilReset,
        DateTime CycleEndDate
    );

    public record DeductCreditsRequest(
        Guid UserId,
        string ServiceName,
        string OperationType,
        string Model,
        int PromptTokens,
        int CompletionTokens,
        string? RelatedEntityId
    );

    public record CreditOperationResult(
        bool Success,
        string Message,
        int RemainingTokens,
        int TokensDeducted
    );

    public record AIOperationLogDTO(
        Guid Id,
        Guid UserId,
        string ServiceName,
        string OperationType,
        string Model,
        int PromptTokens,
        int CompletionTokens,
        int TotalTokens,
        int TokenCost,
        decimal EstimatedMonetaryCost,
        string? RelatedEntityId,
        DateTime CreatedAt
    );
}

