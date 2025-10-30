using AIOCRService.Application.Interfaces;
using AIOCRService.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace AIOCRService.Infrastructure.Services
{
    public class GoogleAIService : IExternalAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAIService> _logger;

        public GoogleAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ProcessTextWithAIAsync(string extractedText, string prompt, string dataType)
        {
            try
            {
                // Validate input data
                if (!IsValidInput(extractedText, prompt, dataType))
                {
                    return GetRejectionResponse(dataType);
                }

                var apiKey = _configuration["GoogleAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Google AI API key not configured");
                }

                // Optimize extracted text for token efficiency
                var optimizedText = TextOptimizationHelper.OptimizeForAI(extractedText, dataType);

                var systemPrompt = AIPromptConfigService.GetSystemPrompt(dataType);
                var userPromptPrefix = AIPromptConfigService.GetUserPrompt(dataType);
                var userPrompt = $"{userPromptPrefix}\n{prompt}\n\nExtracted text: {optimizedText}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"{systemPrompt}\n\n{userPrompt}" }
                            }
                        }
                    },
                    generationConfig = AIPromptConfigService.GetGenerationConfig()
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var model = _configuration["GoogleAI:Model"] ?? "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Google AI API error: {Error}", errorContent);
                    throw new HttpRequestException($"Google AI API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Google AI Response: {Response}", responseContent);
                
                var result = JsonConvert.DeserializeObject<GoogleAIResponse>(responseContent);

                var aiResponse = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No response from AI";
                _logger.LogInformation("Parsed AI Response: {ParsedResponse}", aiResponse);
                
                // Return raw text - parsing will be done by AIResponseParser
                return aiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing text with Google AI");
                throw;
            }
        }

        public async Task<string> ProcessImageDirectlyAsync(string imageBase64, string prompt, string dataType)
        {
            try
            {
                var apiKey = _configuration["GoogleAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Google AI API key not configured");
                }

                // Remove data:image/jpeg;base64, prefix if present
                if (imageBase64.Contains(","))
                {
                    imageBase64 = imageBase64.Split(',')[1];
                }

                var systemPrompt = AIPromptConfigService.GetSystemPrompt(dataType);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = $"{systemPrompt}\n\n{prompt}" },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "image/jpeg",
                                        data = imageBase64
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = AIPromptConfigService.GetGenerationConfig()
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var model = _configuration["GoogleAI:Model"] ?? "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

                // Retry logic for transient errors (503 Service Unavailable)
                HttpResponseMessage? response = null;
                int maxRetries = 3;
                int retryDelayMs = 1000; // Start with 1 second

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        _logger.LogInformation("Retrying Google AI API call (attempt {Attempt}/{MaxRetries}) after {Delay}ms delay", 
                            attempt + 1, maxRetries, retryDelayMs);
                        await Task.Delay(retryDelayMs);
                        retryDelayMs *= 2; // Exponential backoff
                    }

                    response = await _httpClient.PostAsync(url, content);

                    // If successful or non-retryable error, break
                    if (response.IsSuccessStatusCode || (int)response.StatusCode != 503)
                    {
                        break;
                    }

                    // Log 503 error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Google AI API returned 503 (attempt {Attempt}/{MaxRetries}): {Error}", 
                        attempt + 1, maxRetries, errorContent);
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    var errorContent = response != null ? await response.Content.ReadAsStringAsync() : "No response";
                    _logger.LogError("Google AI API error after {MaxRetries} attempts: {Error}", maxRetries, errorContent);
                    throw new HttpRequestException($"Google AI API error: {response?.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Google AI Vision Response: {Response}", responseContent);

                var result = JsonConvert.DeserializeObject<GoogleAIResponse>(responseContent);

                // Check for MAX_TOKENS finish reason
                var candidate = result?.Candidates?.FirstOrDefault();
                if (candidate?.FinishReason == "MAX_TOKENS")
                {
                    _logger.LogError("Google AI response was truncated due to MAX_TOKENS limit. ThoughtsTokenCount: {ThoughtsCount}", 
                        result?.UsageMetadata?.ThoughtsTokenCount);
                    throw new InvalidOperationException("AI response was truncated. The document may be too complex or contain too much data. Please try with a simpler document.");
                }

                var aiResponse = candidate?.Content?.Parts?.FirstOrDefault()?.Text;
                if (string.IsNullOrWhiteSpace(aiResponse))
                {
                    _logger.LogError("No text content in AI response. FinishReason: {FinishReason}", candidate?.FinishReason);
                    throw new InvalidOperationException("AI returned an empty response. Please try again or use a different document.");
                }
                
                _logger.LogInformation("Parsed AI Vision Response: {ParsedResponse}", aiResponse);

                // Return raw text - parsing will be done by AIResponseParser
                return aiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image with Google AI Vision");
                throw;
            }
        }

        public async Task<string> GenerateSummaryAsync(string content)
        {
            var prompt = "Tóm tắt ngắn gọn nội dung sau bằng tiếng Việt:";
            return await ProcessTextWithAIAsync(content, prompt, "summary");
        }

        public async Task<string> GenerateInsightsAsync(string content, string dataType)
        {
            var prompt = AIPromptConfigService.GetUserPrompt(dataType);
            return await ProcessTextWithAIAsync(content, prompt, dataType);
        }

        private static string OptimizeTextForAI(string extractedText, string dataType)
        {
            var tokenLimits = AIPromptConfigService.GetTokenLimits();
            var maxInputChars = tokenLimits["maxInputChars"];

            if (dataType == "lab_value" || dataType == "lab_result")
            {
                // For InBody reports, extract only key metrics
                var lines = extractedText.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList();

                // Keep lines with important keywords
                var importantKeywords = new[] {
                    "Cân nặng", "Weight", "lbs", "kg",
                    "BMI", "Body Mass Index",
                    "PBF", "Percent Body Fat", "Body Fat", "Khối lượng béo",
                    "SMM", "Skeletal Muscle Mass", "Khối lượng cơ",
                    "BMR", "Basal Metabolic Rate", "Tỷ lệ trao đổi chất",
                    "VFA", "Visceral Fat", "Mỡ nội tạng",
                    "TBW", "Total Body Water", "Tổng lượng nước",
                    "Protein", "Mineral", "Lean Body Mass",
                    "Right Arm", "Left Arm", "Trunk", "Right Leg", "Left Leg",
                    "Tay phải", "Tay trái", "Thân", "Chân phải", "Chân trái"
                };

                var importantLines = lines
                    .Where(line => importantKeywords.Any(keyword => 
                        line.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Limit based on config
                var optimized = string.Join("\n", importantLines);
                if (optimized.Length > maxInputChars)
                {
                    optimized = optimized.Substring(0, maxInputChars);
                }

                return string.IsNullOrWhiteSpace(optimized) ? extractedText : optimized;
            }

            // For other types, limit based on config (3x for detailed docs)
            var otherLimit = maxInputChars * 3;
            if (extractedText.Length > otherLimit)
            {
                return extractedText.Substring(0, otherLimit);
            }

            return extractedText;
        }

        private static bool IsValidInput(string extractedText, string prompt, string dataType)
        {
            // Check if extracted text is too short or meaningless
            if (string.IsNullOrWhiteSpace(extractedText) || extractedText.Length < 3)
                return false;

            // Check for common meaningless patterns
            var meaninglessPatterns = new[]
            {
                "string", "test", "hello", "world", "abc", "123", "sample",
                "lorem", "ipsum", "dummy", "placeholder", "example"
            };

            var lowerText = extractedText.ToLower();
            if (meaninglessPatterns.Any(pattern => lowerText.Contains(pattern)))
                return false;

            // Check for inappropriate content
            var inappropriateKeywords = new[]
            {
                "porn", "sex", "adult", "xxx", "nsfw", "violence", "hate",
                "spam", "scam", "fake", "illegal", "drug", "weapon"
            };

            if (inappropriateKeywords.Any(keyword => lowerText.Contains(keyword)))
                return false;

            // Check if prompt is relevant to data type
            if (!IsPromptRelevant(prompt, dataType))
                return false;

            return true;
        }

        private static bool IsPromptRelevant(string prompt, string dataType)
        {
            var lowerPrompt = prompt.ToLower();
            
            return dataType switch
            {
                "nutrition" => lowerPrompt.Contains("dinh dưỡng") || lowerPrompt.Contains("thực phẩm") || 
                              lowerPrompt.Contains("calorie") || lowerPrompt.Contains("nutrition") ||
                              lowerPrompt.Contains("ăn uống") || lowerPrompt.Contains("thức ăn"),
                "medical" => lowerPrompt.Contains("y tế") || lowerPrompt.Contains("sức khỏe") || 
                            lowerPrompt.Contains("bệnh") || lowerPrompt.Contains("medical") ||
                            lowerPrompt.Contains("health") || lowerPrompt.Contains("doctor"),
                "medication" => lowerPrompt.Contains("thuốc") || lowerPrompt.Contains("medication") || 
                               lowerPrompt.Contains("dược") || lowerPrompt.Contains("drug") ||
                               lowerPrompt.Contains("prescription"),
                "lab_value" => lowerPrompt.Contains("xét nghiệm") || lowerPrompt.Contains("kết quả") || 
                              lowerPrompt.Contains("lab") || lowerPrompt.Contains("test") ||
                              lowerPrompt.Contains("inbody") || lowerPrompt.Contains("đo"),
                _ => true // Allow general prompts for unknown types
            };
        }

        private static string GetRejectionResponse(string dataType)
        {
            return dataType switch
            {
                "nutrition" => @"{
  ""summary"": ""Không thể phân tích dữ liệu dinh dưỡng"",
  ""keyNutrients"": [],
  ""calories"": ""N/A"",
  ""recommendations"": [""Vui lòng cung cấp hình ảnh nhãn thực phẩm rõ ràng""],
  ""healthScore"": ""N/A"",
  ""warnings"": [""Dữ liệu không phù hợp hoặc không đủ thông tin""]
}",
                "medical" => @"{
  ""summary"": ""Không thể phân tích tài liệu y tế"",
  ""keyFindings"": [],
  ""recommendations"": [""Vui lòng cung cấp tài liệu y tế hợp lệ""],
  ""nextSteps"": [""Tham khảo ý kiến bác sĩ để được tư vấn chính xác""],
  ""urgency"": ""N/A"",
  ""disclaimer"": ""Lưu ý: Tham khảo ý kiến bác sĩ""
}",
                "medication" => @"{
  ""summary"": ""Không thể phân tích thông tin thuốc"",
  ""medicationName"": ""N/A"",
  ""dosage"": ""N/A"",
  ""instructions"": [""Vui lòng cung cấp thông tin thuốc rõ ràng""],
  ""sideEffects"": [],
  ""warnings"": [""Dữ liệu không phù hợp hoặc không đủ thông tin""],
  ""disclaimer"": ""Lưu ý: Tuân thủ chỉ định bác sĩ""
}",
                "lab_value" => @"{
  ""summary"": ""Không thể phân tích kết quả xét nghiệm"",
  ""keyValues"": [],
  ""normalRanges"": [],
  ""abnormalities"": [],
  ""recommendations"": [""Vui lòng cung cấp kết quả xét nghiệm rõ ràng""],
  ""disclaimer"": ""Lưu ý: Tham khảo ý kiến bác sĩ""
}",
                _ => @"{
  ""summary"": ""Không thể phân tích dữ liệu"",
  ""keyPoints"": [],
  ""insights"": [],
  ""recommendations"": [""Vui lòng cung cấp dữ liệu phù hợp và rõ ràng""]
}"
            };
        }
    }

    // Response models for Google AI API
    public class GoogleAIResponse
    {
        public Candidate[]? Candidates { get; set; }
        public UsageMetadata? UsageMetadata { get; set; }
    }

    public class Candidate
    {
        public Content? Content { get; set; }
        public string? FinishReason { get; set; }
    }

    public class Content
    {
        public Part[]? Parts { get; set; }
    }

    public class Part
    {
        public string? Text { get; set; }
    }

    public class UsageMetadata
    {
        public int ThoughtsTokenCount { get; set; }
        public int PromptTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }
}
