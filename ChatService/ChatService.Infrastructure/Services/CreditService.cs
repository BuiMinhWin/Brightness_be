using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Services
{
    public class CreditService : ICreditService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CreditService> _logger;
        private readonly string _billingApiBaseUrl;
        private readonly string? _gatewayHeaderName;
        private readonly string? _gatewayHeaderValue;

        public CreditService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CreditService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _billingApiBaseUrl = configuration["Services:BillingAPI"]?.TrimEnd('/') 
                ?? throw new InvalidOperationException("BillingAPI URL not configured");
            _gatewayHeaderName = configuration["Services:BillingAPIHeader:Name"];
            _gatewayHeaderValue = configuration["Services:BillingAPIHeader:Value"];
        }

        public async Task<AICreditBalanceDTO> GetCreditBalanceAsync(Guid userId)
        {
            var url = $"{_billingApiBaseUrl}/AICredits/{userId}/balance/schedule"; // Use unified schedule pool
            
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddGatewayHeader(request);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BillingApiResponse<AICreditBalanceDTO>>();
                return result?.Data ?? throw new InvalidOperationException("Failed to get credit balance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit balance for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CreditOperationResult> DeductCreditsAsync(
            Guid userId,
            string operationType,
            string model,
            int promptTokens,
            int completionTokens,
            string? relatedEntityId = null)
        {
            var url = $"{_billingApiBaseUrl}/AICredits/deduct";
            
            try
            {
                var request = new
                {
                    userId,
                    serviceName = "schedule", // Use unified schedule pool for all AI features
                    operationType,
                    model,
                    promptTokens,
                    completionTokens,
                    relatedEntityId
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(request),
                        Encoding.UTF8,
                        "application/json")
                };
                AddGatewayHeader(httpRequest);

                var response = await _httpClient.SendAsync(httpRequest);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BillingApiResponse<CreditOperationResult>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode)
                {
                    return result?.Data ?? new CreditOperationResult(
                        false,
                        $"Failed to deduct credits: {response.StatusCode}",
                        0,
                        0);
                }

                return result?.Data ?? throw new InvalidOperationException("Invalid response from BillingAPI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting credits for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CanAffordOperationAsync(Guid userId, int estimatedTokens)
        {
            var url = $"{_billingApiBaseUrl}/AICredits/{userId}/can-afford?serviceName=chat&estimatedTokens={estimatedTokens}";
            
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddGatewayHeader(request);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BillingApiResponse<bool>>();
                return result?.CanAfford ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking affordability for user {UserId}", userId);
                return false;
            }
        }

        private void AddGatewayHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(_gatewayHeaderName) && !string.IsNullOrWhiteSpace(_gatewayHeaderValue))
            {
                request.Headers.TryAddWithoutValidation(_gatewayHeaderName, _gatewayHeaderValue);
            }
        }

        private class BillingApiResponse<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public bool CanAfford { get; set; }
        }
    }
}

