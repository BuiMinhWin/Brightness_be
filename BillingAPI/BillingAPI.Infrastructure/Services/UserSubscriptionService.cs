using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
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
        private readonly string? _gatewayHeaderName;
        private readonly string? _gatewayHeaderValue;

        public UserSubscriptionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<UserSubscriptionService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuredUrl = configuration["Services:AuthenticationAPI"];
            if (string.IsNullOrWhiteSpace(configuredUrl))
            {
                throw new InvalidOperationException("Authentication API base URL is not configured. Set Services:AuthenticationAPI.");
            }

            _authApiBaseUrl = configuredUrl.TrimEnd('/');
            _gatewayHeaderName = configuration["Services:AuthenticationAPIHeader:Name"];
            _gatewayHeaderValue = configuration["Services:AuthenticationAPIHeader:Value"];

            _logger.LogInformation("UserSubscriptionService initialized with AuthenticationAPI URL: {Url}", _authApiBaseUrl);
        }

        public async Task<UserProfileDTO?> GetUserProfileAsync(Guid userId)
        {
            var requestUrl = $"{_authApiBaseUrl}/{userId}";
            try
            {
                _logger.LogInformation("Fetching user profile for {UserId} from {Url}", userId, requestUrl);

                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                if (!string.IsNullOrWhiteSpace(_gatewayHeaderName) && !string.IsNullOrWhiteSpace(_gatewayHeaderValue))
                {
                    requestMessage.Headers.TryAddWithoutValidation(_gatewayHeaderName, _gatewayHeaderValue);
                }

                using var response = await _httpClient.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Failed to fetch user {UserId} profile. Status: {StatusCode}, Url: {Url}, Error: {Error}",
                        userId,
                        response.StatusCode,
                        requestUrl,
                        error);
                    return null;
                }

                var profile = await response.Content.ReadFromJsonAsync<UserProfileDTO>();
                if (profile is null)
                {
                    _logger.LogWarning("User {UserId} profile response was empty at {Url}", userId, requestUrl);
                }
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching user {UserId} profile from {Url}", userId, requestUrl);
                return null;
            }
        }

        public async Task<bool> UpdateUserSubscriptionAsync(Guid userId, Guid subscriptionPlanId)
        {
            var requestUrl = $"{_authApiBaseUrl}/{userId}/subscription";
            try
            {
                _logger.LogInformation(
                    "Updating user {UserId} subscription to plan {PlanId}. URL: {Url}",
                    userId,
                    subscriptionPlanId,
                    requestUrl);

                var updateRequest = new
                {
                    subscriptionPlanId = subscriptionPlanId
                };

                using var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUrl)
                {
                    Content = JsonContent.Create(updateRequest)
                };

                if (!string.IsNullOrWhiteSpace(_gatewayHeaderName) && !string.IsNullOrWhiteSpace(_gatewayHeaderValue))
                {
                    requestMessage.Headers.TryAddWithoutValidation(_gatewayHeaderName, _gatewayHeaderValue);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(
                        "Successfully updated user {UserId} subscription to plan {PlanId}. Response: {Response}",
                        userId,
                        subscriptionPlanId,
                        responseContent);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to update user {UserId} subscription. Status: {Status}, URL: {Url}, Error: {Error}",
                    userId,
                    response.StatusCode,
                    requestUrl,
                    errorContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception updating user {UserId} subscription to plan {PlanId} {url}",
                    userId,
                    subscriptionPlanId,
                    requestUrl);
                return false;
            }
        }
    }
}
