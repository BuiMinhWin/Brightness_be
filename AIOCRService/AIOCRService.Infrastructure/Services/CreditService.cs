using AIOCRService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AIOCRService.Infrastructure.Services;

public class CreditService : ICreditService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreditService> _logger;
    private readonly string _billingApiUrl;

    public CreditService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CreditService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _billingApiUrl = configuration["Services:BillingAPI"] ?? "http://billing-api:8080";
    }

    public async Task DeductCreditsAsync(
        Guid userId,
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
                UserId = userId,
                ServiceName = "schedule", // Use unified schedule pool for all AI features
                OperationType = operationType,
                Model = modelUsed,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                RelatedEntityId = referenceId
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add Api-Gateway header for service-to-service authentication
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_billingApiUrl}/api/AICredits/deduct")
            {
                Content = content
            };
            requestMessage.Headers.Add("Api-Gateway", "internal-service");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to deduct credits for user {UserId}. Status: {StatusCode}, Error: {Error}",
                    userId, response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogInformation(
                    "Successfully deducted {TotalTokens} tokens for user {UserId} (operation: {OperationType})",
                    promptTokens + completionTokens, userId, operationType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deducting credits for user {UserId}", userId);
            // Don't throw - allow OCR operation to complete even if credit deduction fails
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
}

