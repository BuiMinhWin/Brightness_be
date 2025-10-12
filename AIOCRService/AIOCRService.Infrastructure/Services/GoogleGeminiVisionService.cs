using AIOCRService.Application.Interfaces;
using AIOCRService.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AIOCRService.Infrastructure.Services
{
    /// <summary>
    /// Uses Google Gemini Vision API to read medical documents and extract structured data in one call
    /// </summary>
    public class GoogleGeminiVisionService : IExternalOCRService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleGeminiVisionService> _logger;

        public GoogleGeminiVisionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleGeminiVisionService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ExtractTextFromImageAsync(string imageBase64, string contentType)
        {
            try
            {
                var apiKey = _configuration["GoogleAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Google AI API key not configured");
                }

                // Determine MIME type from content type
                var mimeType = contentType.ToLower() switch
                {
                    "image/jpeg" or "image/jpg" => "image/jpeg",
                    "image/png" => "image/png",
                    "image/webp" => "image/webp",
                    "application/pdf" => "application/pdf",
                    _ => "image/jpeg"
                };

                // Use Gemini 2.5 Flash with vision for fast, accurate OCR
                var model = _configuration["GoogleAI:Model"] ?? "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new
                                {
                                    text = @"Extract ALL text from this image exactly as it appears. 
Include numbers, labels, values, units, dates, and any other text.
Preserve the layout and structure as much as possible.
Return only the extracted text, nothing else."
                                },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = mimeType,
                                        data = imageBase64
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1,
                        topK = 32,
                        topP = 1,
                        maxOutputTokens = 4096
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending image to Gemini Vision API (size: {Size} bytes)", imageBase64.Length);
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

                var extractedText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(extractedText))
                {
                    _logger.LogWarning("No text detected in image");
                    return "No text detected in image";
                }

                _logger.LogInformation("Successfully extracted {Length} characters from image", extractedText.Length);
                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image using Gemini Vision");
                throw;
            }
        }

        public async Task<string> ExtractTextFromImageUrlAsync(string imageUrl)
        {
            try
            {
                // Download image from URL and convert to base64
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                var base64Image = Convert.ToBase64String(imageBytes);

                // Determine content type from URL extension
                var extension = Path.GetExtension(imageUrl).ToLower();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".pdf" => "application/pdf",
                    _ => "image/jpeg"
                };

                return await ExtractTextFromImageAsync(base64Image, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image URL");
                throw;
            }
        }

        /// <summary>
        /// Process image directly with structured data extraction (one-shot)
        /// </summary>
        public async Task<string> ExtractStructuredDataFromImageAsync(string imageBase64, string contentType, string dataType, string prompt)
        {
            try
            {
                var apiKey = _configuration["GoogleAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Google AI API key not configured");
                }

                var mimeType = contentType.ToLower() switch
                {
                    "image/jpeg" or "image/jpg" => "image/jpeg",
                    "image/png" => "image/png",
                    "image/webp" => "image/webp",
                    "application/pdf" => "application/pdf",
                    _ => "image/jpeg"
                };

                var model = _configuration["GoogleAI:Model"] ?? "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var systemPrompt = AIPromptConfigService.GetSystemPrompt(dataType);
                var userPromptPrefix = AIPromptConfigService.GetUserPrompt(dataType);
                var fullPrompt = $"{systemPrompt}\n\n{userPromptPrefix}\n{prompt}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = fullPrompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = mimeType,
                                        data = imageBase64
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        topK = 32,
                        topP = 1,
                        maxOutputTokens = 4096
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

                var structuredData = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(structuredData))
                {
                    _logger.LogWarning("No structured data extracted from image");
                    return "{}";
                }

                return structuredData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting structured data from image using Gemini Vision");
                throw;
            }
        }
    }

    // Response models for Gemini API
    public class GeminiResponse
    {
        public GeminiCandidate[]? Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[]? Parts { get; set; }
    }

    public class GeminiPart
    {
        public string? Text { get; set; }
    }
}

