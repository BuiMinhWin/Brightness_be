using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface IAICreditPoolRepository
    {
        Task<AICreditPool?> GetByUserAndServiceAsync(Guid userId, string serviceName);
        Task<List<AICreditPool>> GetByUserIdAsync(Guid userId);
        Task<AICreditPool> CreateAsync(AICreditPool pool);
        Task UpdateAsync(AICreditPool pool);
        Task<bool> ExistsAsync(Guid userId, string serviceName);
    }
}

