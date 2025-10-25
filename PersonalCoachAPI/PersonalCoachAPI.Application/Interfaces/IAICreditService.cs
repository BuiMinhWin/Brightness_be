using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IAICreditService
    {
        /// <summary>
        /// Get credit balance for a user
        /// </summary>
        Task<AICreditBalanceDTO> GetCreditBalanceAsync(Guid userId);

        /// <summary>
        /// Check if user can afford a specific operation
        /// </summary>
        Task<bool> CanAffordOperationAsync(Guid userId, AIOperationType operationType);

        /// <summary>
        /// Deduct credits for an operation
        /// </summary>
        Task DeductCreditsAsync(Guid userId, AIOperationType operationType, Guid? relatedScheduleId = null);

        /// <summary>
        /// Get credit usage history for a user
        /// </summary>
        Task<List<CreditHistoryItemDTO>> GetCreditHistoryAsync(Guid userId, int limit = 50);

        /// <summary>
        /// Get credit cost for an operation type
        /// </summary>
        int GetOperationCost(AIOperationType operationType);

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        Task<AIUsageLimitsDTO> GetUsageLimitsAsync(Guid userId);
    }
}

