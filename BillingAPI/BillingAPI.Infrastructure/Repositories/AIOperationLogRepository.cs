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
    public class AIOperationLogRepository : IAIOperationLogRepository
    {
        private readonly BillingDbContext _context;

        public AIOperationLogRepository(BillingDbContext context)
        {
            _context = context;
        }

        public async Task<AIOperationLog> CreateAsync(AIOperationLog log)
        {
            _context.AIOperationLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<List<AIOperationLog>> GetByUserIdAsync(Guid userId, string? serviceName, int limit)
        {
            var query = _context.AIOperationLogs
                .Where(log => log.UserId == userId);

            if (!string.IsNullOrEmpty(serviceName))
            {
                query = query.Where(log => log.ServiceName == serviceName);
            }

            return await query
                .OrderByDescending(log => log.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUsedTokensInCycleAsync(Guid creditPoolId, DateTime cycleStart, DateTime cycleEnd)
        {
            return await _context.AIOperationLogs
                .Where(log => log.CreditPoolId == creditPoolId 
                    && log.CreatedAt >= cycleStart 
                    && log.CreatedAt < cycleEnd)
                .SumAsync(log => log.TokenCost);
        }
    }
}

