using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Application.Interfaces;
using BillingAPI.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BillingAPI.Application.Services
{
    public class CreditManagementService : ICreditManagementService
    {
        private readonly IAICreditPoolRepository _creditPoolRepository;
        private readonly IAIOperationLogRepository _operationLogRepository;
        private readonly ICreditCostConfigurationRepository _costConfigRepository;
        private readonly IUserSubscriptionService _userSubscriptionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreditManagementService> _logger;

        public CreditManagementService(
            IAICreditPoolRepository creditPoolRepository,
            IAIOperationLogRepository operationLogRepository,
            ICreditCostConfigurationRepository costConfigRepository,
            IUserSubscriptionService userSubscriptionService,
            IConfiguration configuration,
            ILogger<CreditManagementService> logger)
        {
            _creditPoolRepository = creditPoolRepository;
            _operationLogRepository = operationLogRepository;
            _costConfigRepository = costConfigRepository;
            _userSubscriptionService = userSubscriptionService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AICreditBalanceDTO> GetCreditBalanceAsync(Guid userId, string serviceName)
        {
            var pool = await _creditPoolRepository.GetByUserAndServiceAsync(userId, serviceName);
            
            if (pool == null)
            {
                // Initialize pool if it doesn't exist
                // Default to free tier - can be upgraded later through subscription updates
                var tier = "free";
                await InitializeCreditPoolsForUserAsync(userId, tier);
                pool = await _creditPoolRepository.GetByUserAndServiceAsync(userId, serviceName);
            }

            if (pool == null)
            {
                throw new InvalidOperationException($"Failed to initialize credit pool for user {userId} and service {serviceName}");
            }

            // Check if cycle has expired and reset if needed
            if (DateTime.UtcNow >= pool.CycleEndDate)
            {
                await ResetCreditPoolAsync(pool);
            }

            var usedTokens = pool.MonthlyTokenLimit - pool.RemainingTokens;
            var daysUntilReset = (int)Math.Ceiling((pool.CycleEndDate - DateTime.UtcNow).TotalDays);

            return new AICreditBalanceDTO(
                pool.UserId,
                pool.ServiceName,
                pool.RemainingTokens,
                pool.MonthlyTokenLimit,
                usedTokens,
                pool.SubscriptionTier,
                Math.Max(0, daysUntilReset),
                pool.CycleEndDate
            );
        }

        public async Task<CreditOperationResult> DeductCreditsAsync(DeductCreditsRequest request)
        {
            var pool = await _creditPoolRepository.GetByUserAndServiceAsync(request.UserId, request.ServiceName);
            
            if (pool == null)
            {
                return new CreditOperationResult(
                    false,
                    $"Credit pool not found for user {request.UserId} and service {request.ServiceName}",
                    0,
                    0
                );
            }

            // Check if cycle has expired and reset if needed
            if (DateTime.UtcNow >= pool.CycleEndDate)
            {
                await ResetCreditPoolAsync(pool);
            }

            // Get cost multiplier
            var costConfig = await _costConfigRepository.GetByServiceAndOperationAsync(
                request.ServiceName, request.OperationType);
            
            var multiplier = costConfig?.BaseTokenMultiplier ?? 1.0m;
            var totalTokens = request.PromptTokens + request.CompletionTokens;
            var tokenCost = (int)Math.Ceiling(totalTokens * multiplier);

            // Check if user has enough credits
            if (pool.RemainingTokens < tokenCost)
            {
                return new CreditOperationResult(
                    false,
                    $"Insufficient credits. Required: {tokenCost}, Available: {pool.RemainingTokens}",
                    pool.RemainingTokens,
                    0
                );
            }

            // Deduct credits
            pool.RemainingTokens -= tokenCost;
            pool.UpdatedAt = DateTime.UtcNow;
            await _creditPoolRepository.UpdateAsync(pool);

            // Log the operation
            var monetaryCost = CalculateMonetaryCost(totalTokens, request.Model);
            var operationLog = new AIOperationLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CreditPoolId = pool.Id,
                ServiceName = request.ServiceName,
                OperationType = request.OperationType,
                Model = request.Model,
                PromptTokens = request.PromptTokens,
                CompletionTokens = request.CompletionTokens,
                TotalTokens = totalTokens,
                TokenCost = tokenCost,
                EstimatedMonetaryCost = monetaryCost,
                RelatedEntityId = request.RelatedEntityId,
                CreatedAt = DateTime.UtcNow
            };

            await _operationLogRepository.CreateAsync(operationLog);

            _logger.LogInformation(
                "Deducted {TokenCost} tokens from {ServiceName} pool for user {UserId}. Remaining: {Remaining}",
                tokenCost, request.ServiceName, request.UserId, pool.RemainingTokens);

            return new CreditOperationResult(
                true,
                "Credits deducted successfully",
                pool.RemainingTokens,
                tokenCost
            );
        }

        public async Task<bool> CanAffordOperationAsync(Guid userId, string serviceName, int estimatedTokens)
        {
            var pool = await _creditPoolRepository.GetByUserAndServiceAsync(userId, serviceName);
            
            if (pool == null)
            {
                return false;
            }

            // Check if cycle has expired and reset if needed
            if (DateTime.UtcNow >= pool.CycleEndDate)
            {
                await ResetCreditPoolAsync(pool);
            }

            return pool.RemainingTokens >= estimatedTokens;
        }

        public async Task ResetUserCreditsAsync(Guid userId, DateTime resetDate)
        {
            var pools = await _creditPoolRepository.GetByUserIdAsync(userId);
            
            foreach (var pool in pools)
            {
                await ResetCreditPoolAsync(pool);
            }
        }

        public async Task<List<AIOperationLogDTO>> GetUsageHistoryAsync(Guid userId, string? serviceName, int limit)
        {
            var logs = await _operationLogRepository.GetByUserIdAsync(userId, serviceName, limit);
            
            return logs.Select(log => new AIOperationLogDTO(
                log.Id,
                log.UserId,
                log.ServiceName,
                log.OperationType,
                log.Model,
                log.PromptTokens,
                log.CompletionTokens,
                log.TotalTokens,
                log.TokenCost,
                log.EstimatedMonetaryCost,
                log.RelatedEntityId,
                log.CreatedAt
            )).ToList();
        }

        public async Task InitializeCreditPoolsForUserAsync(Guid userId, string subscriptionTier)
        {
            var services = new[] { "chat", "schedule", "ocr" };
            
            foreach (var service in services)
            {
                var exists = await _creditPoolRepository.ExistsAsync(userId, service);
                if (exists)
                {
                    continue;
                }

                var limit = GetMonthlyTokenLimit(service, subscriptionTier);
                var now = DateTime.UtcNow;
                var cycleEnd = now.AddMonths(1);

                var pool = new AICreditPool
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ServiceName = service,
                    SubscriptionTier = subscriptionTier,
                    MonthlyTokenLimit = limit,
                    RemainingTokens = limit,
                    CycleStartDate = now,
                    CycleEndDate = cycleEnd,
                    CreatedAt = now
                };

                await _creditPoolRepository.CreateAsync(pool);
                _logger.LogInformation(
                    "Initialized {Service} credit pool for user {UserId} with {Limit} tokens",
                    service, userId, limit);
            }
        }

        private async Task ResetCreditPoolAsync(AICreditPool pool)
        {
            var now = DateTime.UtcNow;
            pool.CycleStartDate = now;
            pool.CycleEndDate = now.AddMonths(1);
            pool.RemainingTokens = pool.MonthlyTokenLimit;
            pool.UpdatedAt = now;
            
            await _creditPoolRepository.UpdateAsync(pool);
            
            _logger.LogInformation(
                "Reset credit pool for user {UserId}, service {ServiceName}. New cycle: {Start} to {End}",
                pool.UserId, pool.ServiceName, pool.CycleStartDate, pool.CycleEndDate);
        }

        private int GetMonthlyTokenLimit(string serviceName, string subscriptionTier)
        {
            var tierLower = subscriptionTier.ToLower();
            string configKey = tierLower switch
            {
                "free" => "FreeTier",
                "basic" => "BasicTier",
                "premium" => "PremiumTier",
                _ => "PremiumTier" // Default to premium for unknown tiers
            };
            
            var serviceKey = serviceName == "chat" ? "ChatMonthlyTokens" : "ScheduleMonthlyTokens";
            
            return _configuration.GetValue<int>($"CreditSystem:{configKey}:{serviceKey}", 
                serviceName == "chat" ? 10000 : 5000);
        }

        private decimal CalculateMonetaryCost(int totalTokens, string model)
        {
            // Simplified cost calculation
            // For Gemini Flash: ~$0.075 per 1M input tokens, $0.30 per 1M output tokens
            // Average: ~$0.15 per 1M tokens
            var costPer1MTokens = model.Contains("gemini") ? 0.15m : 0.15m;
            return (totalTokens / 1_000_000m) * costPer1MTokens;
        }
    }
}

