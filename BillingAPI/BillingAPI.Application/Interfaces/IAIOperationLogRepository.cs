using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface IAIOperationLogRepository
    {
        Task<AIOperationLog> CreateAsync(AIOperationLog log);
        Task<List<AIOperationLog>> GetByUserIdAsync(Guid userId, string? serviceName, int limit);
        Task<int> GetUsedTokensInCycleAsync(Guid creditPoolId, DateTime cycleStart, DateTime cycleEnd);
    }
}

