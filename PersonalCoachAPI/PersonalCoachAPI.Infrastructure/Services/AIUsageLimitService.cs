using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class AIUsageLimitService : IAIUsageLimitService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<AIUsageLimitService> _logger;
        private readonly int _freeMonthlyLimit;
        private readonly int _paidMonthlyLimit;

        public AIUsageLimitService(
            PersonalCoachDbContext context,
            ILogger<AIUsageLimitService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            
            _freeMonthlyLimit = int.Parse(configuration["AIGenerationLimits:FreeMonthly"] 
                ?? configuration["AI_GENERATION_LIMIT_FREE"] 
                ?? "2");
                
            _paidMonthlyLimit = int.Parse(configuration["AIGenerationLimits:PaidMonthly"] 
                ?? configuration["AI_GENERATION_LIMIT_PAID"] 
                ?? "50");
        }

        public async Task<AIUsageLimitsDTO> GetUsageLimitsAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            // Get user to check subscription and account creation date
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception($"User {userId} not found");
            }

            var isPaidUser = user.SubscriptionPlanId != null;
            var monthlyLimit = isPaidUser ? _paidMonthlyLimit : _freeMonthlyLimit;
            var subscriptionTier = isPaidUser ? "paid" : "free";

            // Calculate billing cycle based on account creation date (day of month)
            // This mimics real subscription behavior: if you sign up on the 15th, 
            // your billing cycle renews on the 15th of each month
            var cycleStartDay = user.CreatedAt.Day;
            
            // Find the current billing cycle period
            DateTime currentCycleStart;
            DateTime nextCycleStart;
            
            // If we're past the cycle day this month, current cycle started this month
            if (now.Day >= cycleStartDay)
            {
                currentCycleStart = new DateTime(now.Year, now.Month, cycleStartDay, 0, 0, 0, DateTimeKind.Utc);
                // Next cycle is next month
                var nextMonth = now.AddMonths(1);
                nextCycleStart = new DateTime(nextMonth.Year, nextMonth.Month, 
                    Math.Min(cycleStartDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)), 
                    0, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                // Current cycle started last month
                var lastMonth = now.AddMonths(-1);
                currentCycleStart = new DateTime(lastMonth.Year, lastMonth.Month,
                    Math.Min(cycleStartDay, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)),
                    0, 0, 0, DateTimeKind.Utc);
                nextCycleStart = new DateTime(now.Year, now.Month, cycleStartDay, 0, 0, 0, DateTimeKind.Utc);
            }

            // Get usage count for current billing cycle
            var usedCount = await _context.AIGenerationUsages
                .Where(u => u.UserId == userId && u.CreatedAt >= currentCycleStart && u.CreatedAt < nextCycleStart)
                .CountAsync();

            var remaining = Math.Max(0, monthlyLimit - usedCount);
            var canGenerate = remaining > 0;
            var daysUntilReset = (int)Math.Ceiling((nextCycleStart - now).TotalDays);

            _logger.LogInformation(
                "AI Usage for user {UserId}: {Used}/{Limit} ({Tier}), Cycle: {Start} to {End}, Days until reset: {Days}",
                userId, usedCount, monthlyLimit, subscriptionTier, 
                currentCycleStart.ToString("yyyy-MM-dd"), nextCycleStart.ToString("yyyy-MM-dd"), daysUntilReset);

            return new AIUsageLimitsDTO(
                RemainingGenerations: remaining,
                MonthlyLimit: monthlyLimit,
                CanGenerate: canGenerate,
                UsedThisMonth: usedCount,
                DaysUntilReset: daysUntilReset,
                SubscriptionTier: subscriptionTier
            );
        }

        public async Task<bool> CanGenerateAsync(Guid userId)
        {
            var limits = await GetUsageLimitsAsync(userId);
            return limits.CanGenerate;
        }

        public async Task TrackGenerationAsync(Guid userId, string generationType, Guid? relatedScheduleId = null)
        {
            var now = DateTime.UtcNow;
            
            var usage = new AIGenerationUsage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GenerationType = generationType,
                CreatedAt = now,
                Month = now.Month,
                Year = now.Year,
                RelatedScheduleId = relatedScheduleId
            };

            _context.AIGenerationUsages.Add(usage);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Tracked AI generation for user {UserId}: {Type}",
                userId, generationType);
        }
    }
}




