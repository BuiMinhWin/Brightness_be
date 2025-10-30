using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Domain.Entities;
using BillingAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillingAPI.Infrastructure.Repositories
{
    public class CreditCostConfigurationRepository : ICreditCostConfigurationRepository
    {
        private readonly BillingDbContext _context;

        public CreditCostConfigurationRepository(BillingDbContext context)
        {
            _context = context;
        }

        public async Task<CreditCostConfiguration?> GetByServiceAndOperationAsync(string serviceName, string operationType)
        {
            return await _context.CreditCostConfigurations
                .FirstOrDefaultAsync(c => c.ServiceName == serviceName 
                    && c.OperationType == operationType 
                    && c.IsActive);
        }

        public async Task<List<CreditCostConfiguration>> GetAllActiveAsync()
        {
            return await _context.CreditCostConfigurations
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<CreditCostConfiguration> CreateAsync(CreditCostConfiguration config)
        {
            _context.CreditCostConfigurations.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }
    }
}

