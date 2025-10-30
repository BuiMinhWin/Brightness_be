using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface ICreditCostConfigurationRepository
    {
        Task<CreditCostConfiguration?> GetByServiceAndOperationAsync(string serviceName, string operationType);
        Task<List<CreditCostConfiguration>> GetAllActiveAsync();
        Task<CreditCostConfiguration> CreateAsync(CreditCostConfiguration config);
    }
}

