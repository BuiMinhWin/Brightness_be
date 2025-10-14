using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BillingAPI.Infrastructure.Services
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserSubscriptionService> _logger;
        private readonly string _authApiBaseUrl;

        public UserSubscriptionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<UserSubscriptionService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get AuthenticationAPI URL from configuration
            _authApiBaseUrl = configuration["Services:AuthenticationAPI"]
                ?? "http://authentication-api:8080/api/users";
        }

        public async Task<bool> UpdateUserSubscriptionAsync(Guid userId, Guid subscriptionPlanId)
        {
            try
            {
                _logger.LogInformation(
                    "Updating user {UserId} subscription to plan {PlanId}",
                    userId,
                    subscriptionPlanId);

                var updateRequest = new
                {
                    subscriptionPlanId = subscriptionPlanId
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"{_authApiBaseUrl}/{userId}/subscription",
                    updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully updated user {UserId} subscription to plan {PlanId}",
                        userId,
                        subscriptionPlanId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to update user {UserId} subscription. Status: {Status}, Error: {Error}",
                    userId,
                    response.StatusCode,
                    errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating user {UserId} subscription to plan {PlanId}",
                    userId,
                    subscriptionPlanId);
                return false;
            }
        }
    }
}
