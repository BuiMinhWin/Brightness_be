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
    public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(BillingDbContext context) : base(context)
        {
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var normalized = name.Trim().ToLowerInvariant();
            return await Context.SubscriptionPlans
                .AsNoTracking()
                .AnyAsync(plan => plan.Name.ToLower() == normalized && (!excludeId.HasValue || plan.Id != excludeId));
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync()
        {
            return await Context.SubscriptionPlans
                .AsNoTracking()
                .Where(plan => plan.IsActive)
                .OrderBy(plan => plan.Price)
                .ToListAsync();
        }

        public override async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        {
            return await Context.SubscriptionPlans
                .AsNoTracking()
                .OrderBy(plan => plan.Price)
                .ToListAsync();
        }
    }
}
