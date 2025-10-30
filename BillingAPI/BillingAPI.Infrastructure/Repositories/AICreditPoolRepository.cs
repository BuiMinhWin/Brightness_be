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
    public class AICreditPoolRepository : IAICreditPoolRepository
    {
        private readonly BillingDbContext _context;

        public AICreditPoolRepository(BillingDbContext context)
        {
            _context = context;
        }

        public async Task<AICreditPool?> GetByUserAndServiceAsync(Guid userId, string serviceName)
        {
            return await _context.AICreditPools
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ServiceName == serviceName);
        }

        public async Task<List<AICreditPool>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AICreditPools
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<AICreditPool> CreateAsync(AICreditPool pool)
        {
            _context.AICreditPools.Add(pool);
            await _context.SaveChangesAsync();
            return pool;
        }

        public async Task UpdateAsync(AICreditPool pool)
        {
            _context.AICreditPools.Update(pool);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, string serviceName)
        {
            return await _context.AICreditPools
                .AnyAsync(p => p.UserId == userId && p.ServiceName == serviceName);
        }
    }
}

