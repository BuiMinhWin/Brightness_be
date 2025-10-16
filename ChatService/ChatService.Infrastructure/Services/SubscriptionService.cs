using System;
using System.Threading.Tasks;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            ChatDbContext context,
            ILogger<SubscriptionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserSubscriptionDTO?> GetUserSubscriptionAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Fetching subscription details for user {UserId}", userId);

                // Query database directly - more reliable than HTTP calls
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user?.SubscriptionPlanId == null)
                {
                    _logger.LogInformation("User {UserId} has no subscription plan", userId);
                    return new UserSubscriptionDTO(userId, null, null);
                }

                var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == user.SubscriptionPlanId);
                
                if (plan == null)
                {
                    _logger.LogWarning(
                        "Subscription plan {PlanId} not found for user {UserId}",
                        user.SubscriptionPlanId,
                        userId);
                    return new UserSubscriptionDTO(userId, user.SubscriptionPlanId, null);
                }

                var planDetails = new SubscriptionPlanDetailsDTO(
                    plan.Id,
                    plan.Name,
                    null, // Description not in minimal entity
                    0m,   // Price not in minimal entity
                    "",   // Currency not in minimal entity  
                    0,    // DurationDays not in minimal entity
                    true  // IsActive not in minimal entity
                );

                _logger.LogInformation(
                    "Successfully fetched subscription details for user {UserId}. Plan: {PlanName}",
                    userId,
                    plan.Name);

                return new UserSubscriptionDTO(userId, user.SubscriptionPlanId, planDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching subscription details for user {UserId}",
                    userId);
                return null;
            }
        }

    }
}

