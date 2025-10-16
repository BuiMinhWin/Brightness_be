using System;
using System.Threading.Tasks;
using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    /// <summary>
    /// Service to fetch user subscription information from external services
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Get user's subscription plan details
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User subscription details if exists, null otherwise</returns>
        Task<UserSubscriptionDTO?> GetUserSubscriptionAsync(Guid userId);
    }
}

