using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class AICreditService : IAICreditService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<AICreditService> _logger;
        private readonly int _freeMonthlyCredits;
        private readonly int _basicMonthlyCredits;
        private readonly int _premiumMonthlyCredits;
        private readonly Dictionary<AIOperationType, int> _operationCosts;

        public AICreditService(
            PersonalCoachDbContext context,
            ILogger<AICreditService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;

            // Load credit allocations from configuration
            _freeMonthlyCredits = int.Parse(configuration["AICredits:FreeMonthlyCredits"] ?? "20");
            _basicMonthlyCredits = int.Parse(configuration["AICredits:BasicMonthlyCredits"] ?? "500");
            _premiumMonthlyCredits = int.Parse(configuration["AICredits:PremiumMonthlyCredits"] ?? "1200");

            // Load operation costs
            _operationCosts = new Dictionary<AIOperationType, int>
            {
                { AIOperationType.ScheduleGeneration, int.Parse(configuration["AICredits:OperationCosts:ScheduleGeneration"] ?? "10") },
                { AIOperationType.ScheduleRegeneration, int.Parse(configuration["AICredits:OperationCosts:ScheduleRegeneration"] ?? "10") },
                { AIOperationType.DayModification, int.Parse(configuration["AICredits:OperationCosts:DayModification"] ?? "5") },
                { AIOperationType.OCRProcessing, int.Parse(configuration["AICredits:OperationCosts:OCRProcessing"] ?? "5") },
                { AIOperationType.ChatMessage, int.Parse(configuration["AICredits:OperationCosts:ChatMessage"] ?? "1") }
            };
        }

        public async Task<AICreditBalanceDTO> GetCreditBalanceAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            // Get user to check subscription
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception($"User {userId} not found");
            }

            // Determine subscription tier and monthly credits
            var (subscriptionTier, monthlyCredits) = GetSubscriptionTierInfo(user);

            // Calculate billing cycle
            var (currentCycleStart, nextCycleStart) = CalculateBillingCycle(user.CreatedAt, now);

            // Get total credits used in current billing cycle
            var usedCredits = await _context.AIGenerationUsages
                .Where(u => u.UserId == userId && u.CreatedAt >= currentCycleStart && u.CreatedAt < nextCycleStart)
                .SumAsync(u => u.CreditCost);

            var remainingCredits = Math.Max(0, monthlyCredits - usedCredits);
            var canAffordOperation = remainingCredits > 0;
            var daysUntilReset = (int)Math.Ceiling((nextCycleStart - now).TotalDays);

            // Create cost breakdown dictionary
            var costBreakdown = _operationCosts.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );

            _logger.LogInformation(
                "Credit Balance for user {UserId}: {Remaining}/{Monthly} ({Tier}), Cycle: {Start} to {End}, Days until reset: {Days}",
                userId, remainingCredits, monthlyCredits, subscriptionTier,
                currentCycleStart.ToString("yyyy-MM-dd"), nextCycleStart.ToString("yyyy-MM-dd"), daysUntilReset);

            return new AICreditBalanceDTO(
                RemainingCredits: remainingCredits,
                MonthlyCredits: monthlyCredits,
                CanAffordOperation: canAffordOperation,
                UsedCreditsThisMonth: usedCredits,
                DaysUntilReset: daysUntilReset,
                SubscriptionTier: subscriptionTier,
                CreditCostBreakdown: costBreakdown
            );
        }

        public async Task<bool> CanAffordOperationAsync(Guid userId, AIOperationType operationType)
        {
            var balance = await GetCreditBalanceAsync(userId);
            var operationCost = GetOperationCost(operationType);
            return balance.RemainingCredits >= operationCost;
        }

        public async Task DeductCreditsAsync(Guid userId, AIOperationType operationType, Guid? relatedScheduleId = null)
        {
            var operationCost = GetOperationCost(operationType);
            var canAfford = await CanAffordOperationAsync(userId, operationType);

            if (!canAfford)
            {
                var balance = await GetCreditBalanceAsync(userId);
                throw new InvalidOperationException(
                    $"Insufficient credits. Operation requires {operationCost} credits, but only {balance.RemainingCredits} remaining. " +
                    $"Resets in {balance.DaysUntilReset} days.");
            }

            var now = DateTime.UtcNow;
            var usage = new AIGenerationUsage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GenerationType = operationType.ToString(),
                OperationType = operationType,
                CreditCost = operationCost,
                CreatedAt = now,
                Month = now.Month,
                Year = now.Year,
                RelatedScheduleId = relatedScheduleId
            };

            _context.AIGenerationUsages.Add(usage);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Deducted {Credits} credits for user {UserId}: {Operation}",
                operationCost, userId, operationType);
        }

        public async Task<List<CreditHistoryItemDTO>> GetCreditHistoryAsync(Guid userId, int limit = 50)
        {
            var history = await _context.AIGenerationUsages
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .Take(limit)
                .ToListAsync();

            // Calculate remaining credits after each operation
            var allUsages = await _context.AIGenerationUsages
                .Where(u => u.UserId == userId)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return new List<CreditHistoryItemDTO>();

            var (_, monthlyCredits) = GetSubscriptionTierInfo(user);

            var historyWithRemaining = new List<CreditHistoryItemDTO>();
            var currentCycleStart = DateTime.MinValue;
            var creditsUsedInCycle = 0;

            foreach (var usage in history)
            {
                var (cycleStart, _) = CalculateBillingCycle(user.CreatedAt, usage.CreatedAt);

                // Reset if we've moved to a new billing cycle
                if (cycleStart != currentCycleStart)
                {
                    currentCycleStart = cycleStart;
                    creditsUsedInCycle = allUsages
                        .Where(u => u.CreatedAt >= cycleStart && u.CreatedAt <= usage.CreatedAt)
                        .Sum(u => u.CreditCost);
                }
                else
                {
                    creditsUsedInCycle += usage.CreditCost;
                }

                var remainingAfter = monthlyCredits - creditsUsedInCycle;

                historyWithRemaining.Add(new CreditHistoryItemDTO(
                    Id: usage.Id,
                    OperationType: usage.OperationType.ToString(),
                    CreditCost: usage.CreditCost,
                    RemainingCreditsAfter: remainingAfter,
                    CreatedAt: usage.CreatedAt,
                    RelatedScheduleId: usage.RelatedScheduleId
                ));
            }

            return historyWithRemaining;
        }

        public int GetOperationCost(AIOperationType operationType)
        {
            return _operationCosts.TryGetValue(operationType, out var cost) ? cost : 0;
        }

        // Legacy method for backward compatibility
        public async Task<AIUsageLimitsDTO> GetUsageLimitsAsync(Guid userId)
        {
            var creditBalance = await GetCreditBalanceAsync(userId);

            // Convert credits to "generations" for backward compatibility
            // Assuming 1 generation = 10 credits
            var generationCost = GetOperationCost(AIOperationType.ScheduleGeneration);
            var remainingGenerations = creditBalance.RemainingCredits / generationCost;
            var monthlyGenerations = creditBalance.MonthlyCredits / generationCost;
            var usedGenerations = creditBalance.UsedCreditsThisMonth / generationCost;

            return new AIUsageLimitsDTO(
                RemainingGenerations: remainingGenerations,
                MonthlyLimit: monthlyGenerations,
                CanGenerate: remainingGenerations > 0,
                UsedThisMonth: usedGenerations,
                DaysUntilReset: creditBalance.DaysUntilReset,
                SubscriptionTier: creditBalance.SubscriptionTier
            );
        }

        private (string tier, int credits) GetSubscriptionTierInfo(AuthUser user)
        {
            if (user.SubscriptionPlanId == null)
            {
                return ("free", _freeMonthlyCredits);
            }

            // Map subscription plan IDs to credit tiers
            // These IDs match the ones in insert_subscription_plans.sql
            var premiumPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"); // Premium Monthly
            var premiumYearlyPlanId = Guid.Parse("55555555-5555-5555-5555-555555555555"); // Premium Yearly
            
            if (user.SubscriptionPlanId == premiumPlanId || user.SubscriptionPlanId == premiumYearlyPlanId)
            {
                return ("premium", _premiumMonthlyCredits);
            }

            // All other paid plans are Basic tier
            return ("basic", _basicMonthlyCredits);
        }

        private (DateTime currentCycleStart, DateTime nextCycleStart) CalculateBillingCycle(DateTime accountCreatedAt, DateTime now)
        {
            var cycleStartDay = accountCreatedAt.Day;

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

            return (currentCycleStart, nextCycleStart);
        }
    }
}

