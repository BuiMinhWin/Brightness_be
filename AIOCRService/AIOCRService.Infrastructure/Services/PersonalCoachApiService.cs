using AIOCRService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AIOCRService.Infrastructure.Services
{
    public class PersonalCoachApiService : IPersonalCoachApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PersonalCoachApiService> _logger;
        private readonly string _internalApiKey;
        private readonly string _personalCoachApiUrl;

        public PersonalCoachApiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PersonalCoachApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _internalApiKey = configuration["INTERNAL_API_KEY"] 
                ?? throw new InvalidOperationException("INTERNAL_API_KEY not configured");
            _personalCoachApiUrl = configuration["Services:PersonalCoachAPI"] 
                ?? "http://personal-coach-api:8080";
        }

        public async Task<bool> CanAffordOCRProcessingAsync(Guid userId)
        {
            try
            {
                var requestBody = new { userId, operationType = 5 }; // 5 = OCRProcessing enum value
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_personalCoachApiUrl}/api/schedule/can-afford");
                httpRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CanAffordResponse>(responseContent);
                    return result?.CanAfford ?? false;
                }

                _logger.LogWarning("Failed to check credit affordability for user {UserId}: {StatusCode}", 
                    userId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking credit affordability for user {UserId}", userId);
                return false;
            }
        }

        public async Task DeductOCRProcessingCreditAsync(Guid userId)
        {
            try
            {
                var requestBody = new { userId, operationType = 5 }; // 5 = OCRProcessing enum value
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_personalCoachApiUrl}/api/schedule/deduct-credit");
                httpRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
                    {
                        _logger.LogWarning("Insufficient credits for OCR processing for user {UserId}: {Error}", 
                            userId, errorContent);
                        throw new InvalidOperationException("Insufficient credits for OCR processing");
                    }
                    
                    _logger.LogWarning("Failed to deduct OCR credit for user {UserId}: {StatusCode} - {Error}", 
                        userId, response.StatusCode, errorContent);
                }
                else
                {
                    _logger.LogInformation("Successfully deducted 5 credits for OCR processing from user {UserId}", userId);
                }
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw insufficient credits exception
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting OCR credit for user {UserId}", userId);
            }
        }

        private class CanAffordResponse
        {
            public bool CanAfford { get; set; }
        }
    }
}

