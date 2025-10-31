using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface ICreditManagementService
    {
        Task<AICreditBalanceDTO> GetCreditBalanceAsync(Guid userId, string serviceName);
        Task<CreditOperationResult> DeductCreditsAsync(DeductCreditsRequest request);
        Task<bool> CanAffordOperationAsync(Guid userId, string serviceName, int estimatedTokens);
        Task ResetUserCreditsAsync(Guid userId, DateTime resetDate);
        Task<List<AIOperationLogDTO>> GetUsageHistoryAsync(Guid userId, string? serviceName, int limit);
        Task InitializeCreditPoolsForUserAsync(Guid userId, string subscriptionTier);
    }
}

