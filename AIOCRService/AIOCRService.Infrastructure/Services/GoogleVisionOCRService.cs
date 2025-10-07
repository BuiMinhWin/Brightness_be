using AIOCRService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AIOCRService.Infrastructure.Services
{
    public class GoogleVisionOCRService : IExternalOCRService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleVisionOCRService> _logger;

        public GoogleVisionOCRService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleVisionOCRService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ExtractTextFromImageAsync(string imageBase64, string contentType)
        {
            try
            {
                var apiKey = _configuration["GoogleVision:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Google Vision API key not configured");
                }

                var requestBody = new
                {
                    requests = new[]
                    {
                        new
                        {
                            image = new
                            {
                                content = imageBase64
                            },
                            features = new[]
                            {
                                new
                                {
                                    type = "TEXT_DETECTION",
                                    maxResults = 1
                                }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Google Vision API error: {Error}", errorContent);
                    throw new HttpRequestException($"Google Vision API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GoogleVisionResponse>(responseContent);

                if (result?.Responses?.FirstOrDefault()?.TextAnnotations?.FirstOrDefault()?.Description != null)
                {
                    return result.Responses.First().TextAnnotations.First().Description;
                }

                return "No text detected in image";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image");
                throw;
            }
        }

        public async Task<string> ExtractTextFromImageUrlAsync(string imageUrl)
        {
            try
            {
                var apiKey = _configuration["GoogleVision:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Google Vision API key not configured");
                }

                var requestBody = new
                {
                    requests = new[]
                    {
                        new
                        {
                            image = new
                            {
                                source = new
                                {
                                    imageUri = imageUrl
                                }
                            },
                            features = new[]
                            {
                                new
                                {
                                    type = "TEXT_DETECTION",
                                    maxResults = 1
                                }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Google Vision API error: {Error}", errorContent);
                    throw new HttpRequestException($"Google Vision API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GoogleVisionResponse>(responseContent);

                if (result?.Responses?.FirstOrDefault()?.TextAnnotations?.FirstOrDefault()?.Description != null)
                {
                    return result.Responses.First().TextAnnotations.First().Description;
                }

                return "No text detected in image";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image URL");
                throw;
            }
        }
    }

    // Response models for Google Vision API
    public class GoogleVisionResponse
    {
        public GoogleVisionResponseItem[]? Responses { get; set; }
    }

    public class GoogleVisionResponseItem
    {
        public TextAnnotation[]? TextAnnotations { get; set; }
    }

    public class TextAnnotation
    {
        public string? Description { get; set; }
    }
}

