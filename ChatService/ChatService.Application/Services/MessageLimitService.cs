using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ChatService.Application.Services
{
    public class MessageLimitService : IMessageLimitService
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IConfiguration _configuration;
        private readonly int _freeDailyLimit;
        private readonly int _freeMonthlyLimit;
        private readonly int _premiumDailyLimit;
        private readonly int _premiumMonthlyLimit;

        public MessageLimitService(
            IChatMessageRepository messageRepository,
            IConfiguration configuration)
        {
            _messageRepository = messageRepository;
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

            // For now, assume free tier (can be enhanced to check subscription)
            var dailyLimit = _freeDailyLimit;
            var monthlyLimit = _freeMonthlyLimit;

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

