using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface ICreditService
    {
        Task<AICreditBalanceDTO> GetCreditBalanceAsync(Guid userId);
        Task<CreditOperationResult> DeductCreditsAsync(
            Guid userId, 
            string operationType, 
            string model, 
            int promptTokens, 
            int completionTokens,
            string? relatedEntityId = null);
        Task<bool> CanAffordOperationAsync(Guid userId, int estimatedTokens);
    }

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

    public record CreditOperationResult(
        bool Success,
        string Message,
        int RemainingTokens,
        int TokensDeducted
    );
}

