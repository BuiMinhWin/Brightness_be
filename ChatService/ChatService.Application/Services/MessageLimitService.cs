using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ChatService.Application.Services
{
    public class MessageLimitService : IMessageLimitService
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<MessageLimitService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _freeDailyLimit;
        private readonly int _freeMonthlyLimit;
        private readonly int _premiumDailyLimit;
        private readonly int _premiumMonthlyLimit;

        public MessageLimitService(
            IChatMessageRepository messageRepository,
            ISubscriptionService subscriptionService,
            ILogger<MessageLimitService> logger,
            IConfiguration configuration)
        {
            _messageRepository = messageRepository;
            _subscriptionService = subscriptionService;
            _logger = logger;
            _configuration = configuration;

            _freeDailyLimit = int.Parse(configuration["MessageLimits:FreeDaily"] 
                ?? configuration["MESSAGE_LIMITS_FREE_DAILY"] 
                ?? "10");
                
            _freeMonthlyLimit = int.Parse(configuration["MessageLimits:FreeMonthly"] 
                ?? configuration["MESSAGE_LIMITS_FREE_MONTHLY"] 
                ?? "100");
                
            _premiumDailyLimit = int.Parse(configuration["MessageLimits:PremiumDaily"] 
                ?? configuration["MESSAGE_LIMITS_PREMIUM_DAILY"] 
                ?? "100");
                
            _premiumMonthlyLimit = int.Parse(configuration["MessageLimits:PremiumMonthly"] 
                ?? configuration["MESSAGE_LIMITS_PREMIUM_MONTHLY"] 
                ?? "1000");
        }

        public async Task<MessageLimitsDTO> GetMessageLimitsAsync(Guid userId)
        {
            var todayCount = await _messageRepository.GetTodayMessageCountByUserIdAsync(userId);
            var monthlyCount = await _messageRepository.GetMonthlyMessageCountByUserIdAsync(userId);

            // Check user's subscription to determine limits
            var (dailyLimit, monthlyLimit) = await GetUserLimitsAsync(userId);

            var remainingDaily = Math.Max(0, dailyLimit - todayCount);
            var remainingMonthly = Math.Max(0, monthlyLimit - monthlyCount);

            return new MessageLimitsDTO(
                RemainingDaily: remainingDaily,
                DailyLimit: dailyLimit,
                RemainingMonthly: remainingMonthly,
                MonthlyLimit: monthlyLimit,
                IsLimitReached: remainingDaily <= 0 || remainingMonthly <= 0
            );
        }

        private async Task<(int dailyLimit, int monthlyLimit)> GetUserLimitsAsync(Guid userId)
        {
            try
            {
                var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
                
                // If user has no subscription or plan details are unavailable, use free tier
                if (subscription?.PlanDetails == null)
                {
                    _logger.LogInformation("User {UserId} using free tier limits", userId);
                    return (_freeDailyLimit, _freeMonthlyLimit);
                }

                // Determine limits based on plan name
                var planName = subscription.PlanDetails.Name.ToLowerInvariant();
                
                if (planName.Contains("premium") || planName.Contains("pro") || planName.Contains("paid"))
                {
                    _logger.LogInformation(
                        "User {UserId} using premium tier limits (Plan: {PlanName})",
                        userId,
                        subscription.PlanDetails.Name);
                    return (_premiumDailyLimit, _premiumMonthlyLimit);
                }

                // Default to free tier if plan name doesn't match known premium patterns
                _logger.LogInformation(
                    "User {UserId} using free tier limits (Plan: {PlanName})",
                    userId,
                    subscription.PlanDetails.Name);
                return (_freeDailyLimit, _freeMonthlyLimit);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching subscription limits for user {UserId}, defaulting to free tier",
                    userId);
                return (_freeDailyLimit, _freeMonthlyLimit);
            }
        }

        public async Task<bool> CanSendMessageAsync(Guid userId)
        {
            var limits = await GetMessageLimitsAsync(userId);
            return !limits.IsLimitReached;
        }

        public async Task IncrementMessageCountAsync(Guid userId)
        {
            // The count is incremented automatically when a message is added
            // This method is here for potential future use (e.g., caching)
            await Task.CompletedTask;
        }
    }
}

