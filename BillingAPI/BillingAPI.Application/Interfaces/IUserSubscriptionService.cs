using System;
using System.Threading.Tasks;

namespace BillingAPI.Application.Interfaces
{
    /// <summary>
    /// Service to update user subscription information in AuthenticationAPI
    /// </summary>
    public interface IUserSubscriptionService
    {
        /// <summary>
        /// Update user's subscription plan after successful payment
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="subscriptionPlanId">Subscription Plan ID</param>
        /// <returns>True if update successful, false otherwise</returns>
        Task<bool> UpdateUserSubscriptionAsync(Guid userId, Guid subscriptionPlanId);
    }
}
