using PersonalCoachAPI.Application.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PersonalCoachAPI.Infrastructure.Services;

public class CreditService : ICreditService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreditService> _logger;
    private readonly string _billingApiUrl;

    public CreditService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CreditService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _billingApiUrl = configuration["BillingAPI:BaseUrl"] 
            ?? throw new InvalidOperationException("BillingAPI:BaseUrl not configured");
        
        // Add Api-Gateway header for service-to-service calls
        _httpClient.DefaultRequestHeaders.Add("Api-Gateway", "Internal");
    }

    public async Task<bool> CanAffordOperationAsync(string userId, string serviceName, int estimatedTokens)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_billingApiUrl}/api/AICredits/{userId}/can-afford?serviceName={serviceName}&estimatedTokens={estimatedTokens}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to check affordability for user {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return false; // Conservative: assume cannot afford on error
            }

            var result = await response.Content.ReadFromJsonAsync<CanAffordResponse>();
            return result?.CanAfford ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can afford operation", userId);
            return false; // Conservative: assume cannot afford on error
        }
    }

    public async Task DeductCreditsAsync(
        string userId,
        string operationType,
        string modelUsed,
        int promptTokens,
        int completionTokens,
        string? referenceId = null)
    {
        try
        {
            var request = new DeductCreditsRequest
            {
                UserId = Guid.Parse(userId),
                ServiceName = "schedule", // PersonalCoachAPI always uses 'schedule' service
                OperationType = operationType,
                Model = modelUsed,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                RelatedEntityId = referenceId
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_billingApiUrl}/api/AICredits/deduct", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to deduct credits for user {UserId}. Status: {StatusCode}, Error: {Error}",
                    userId, response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogInformation(
                    "Successfully deducted {Tokens} tokens for user {UserId} ({OperationType})",
                    promptTokens + completionTokens, userId, operationType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deducting credits for user {UserId}", userId);
            // Don't throw - we don't want to fail the operation if credit tracking fails
        }
    }

    public async Task<CreditBalanceDTO> GetCreditBalanceAsync(string userId, string serviceName)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_billingApiUrl}/api/AICredits/{userId}/balance/{serviceName}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get credit balance for user {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                
                // Return default values
                return new CreditBalanceDTO
                {
                    RemainingTokens = 0,
                    MonthlyTokens = 5000,
                    UsedTokens = 0,
                    ResetDate = DateTime.UtcNow.AddMonths(1),
                    SubscriptionTier = "free",
                    CanAfford = false
                };
            }

            var wrapper = await response.Content.ReadFromJsonAsync<CreditBalanceResponseWrapper>();
            var result = wrapper?.Data;
            
            return new CreditBalanceDTO
            {
                RemainingTokens = result?.RemainingTokens ?? 0,
                MonthlyTokens = result?.MonthlyTokens ?? 5000,
                UsedTokens = result?.UsedTokens ?? 0,
                ResetDate = result?.ResetDate ?? DateTime.UtcNow.AddMonths(1),
                SubscriptionTier = result?.SubscriptionTier ?? "free",
                CanAfford = (result?.RemainingTokens ?? 0) > 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit balance for user {UserId}", userId);
            
            // Return default values on error
            return new CreditBalanceDTO
            {
                RemainingTokens = 0,
                MonthlyTokens = 5000,
                UsedTokens = 0,
                ResetDate = DateTime.UtcNow.AddMonths(1),
                SubscriptionTier = "free",
                CanAfford = false
            };
        }
    }

    // DTOs for API communication
    private class DeductCreditsRequest
    {
        public Guid UserId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public string? RelatedEntityId { get; set; }
    }

    private class CanAffordResponse
    {
        public bool Success { get; set; }
        public bool CanAfford { get; set; }
        public int? RemainingTokens { get; set; }
        public int? EstimatedCost { get; set; }
    }

    private class CreditBalanceResponseWrapper
    {
        public bool Success { get; set; }
        public CreditBalanceResponse? Data { get; set; }
    }

    private class CreditBalanceResponse
    {
        public int RemainingTokens { get; set; }
        public int MonthlyTokens { get; set; }
        public int UsedTokens { get; set; }
        public DateTime ResetDate { get; set; }
        public string SubscriptionTier { get; set; } = string.Empty;
    }
}

