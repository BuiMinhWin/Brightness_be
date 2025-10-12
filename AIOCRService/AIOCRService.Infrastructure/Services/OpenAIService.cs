using AIOCRService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace AIOCRService.Infrastructure.Services
{
    public class OpenAIService : IExternalAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OpenAIService> logger)
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

                var apiKey = _configuration["OpenAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenAI API key not configured");
                }

                var systemPrompt = GetSystemPrompt(dataType);
                var userPrompt = $"{prompt}\n\nExtracted text: {extractedText}";

                var requestBody = new
                {
                    model = _configuration["OpenAI:Model"] ?? "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    max_tokens = 2000,
                    temperature = 0.3
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API error: {Error}", errorContent);
                    throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);

                var aiResponse = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response from AI";
                
                // Try to parse as JSON, if it fails, return as is
                try
                {
                    var jsonResponse = JsonConvert.DeserializeObject(aiResponse);
                    return JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                }
                catch
                {
                    return aiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing text with AI");
                throw;
            }
        }

        public Task<string> ProcessImageDirectlyAsync(string imageBase64, string prompt, string dataType)
        {
            // OpenAI doesn't support this yet, use GoogleAI instead
            throw new NotImplementedException("OpenAI doesn't support direct image processing. Use GoogleAI service instead.");
        }

        public async Task<string> GenerateSummaryAsync(string content)
        {
            var prompt = "Tóm tắt ngắn gọn nội dung sau bằng tiếng Việt:";
            return await ProcessTextWithAIAsync(content, prompt, "summary");
        }

        public async Task<string> GenerateInsightsAsync(string content, string dataType)
        {
            var prompt = GetInsightPrompt(dataType);
            return await ProcessTextWithAIAsync(content, prompt, dataType);
        }

        private static string GetSystemPrompt(string dataType)
        {
            return dataType switch
            {
                "nutrition" => @"Bạn là chuyên gia dinh dưỡng. Phân tích thông tin dinh dưỡng và trả về JSON với format:
{
  ""summary"": ""Tóm tắt ngắn gọn về sản phẩm"",
  ""keyNutrients"": [""chất dinh dưỡng 1"", ""chất dinh dưỡng 2""],
  ""calories"": ""số calo"",
  ""recommendations"": [""khuyến nghị 1"", ""khuyến nghị 2""],
  ""healthScore"": ""điểm sức khỏe (1-10)"",
  ""warnings"": [""cảnh báo nếu có""]
}",
                "medical" => @"Bạn là trợ lý y tế. Phân tích tài liệu y tế và trả về JSON với format:
{
  ""summary"": ""Tóm tắt tài liệu y tế"",
  ""keyFindings"": [""phát hiện quan trọng 1"", ""phát hiện quan trọng 2""],
  ""recommendations"": [""khuyến nghị 1"", ""khuyến nghị 2""],
  ""nextSteps"": [""bước tiếp theo 1"", ""bước tiếp theo 2""],
  ""urgency"": ""mức độ khẩn cấp (thấp/trung bình/cao)"",
  ""disclaimer"": ""Lưu ý: Tham khảo ý kiến bác sĩ""
}",
                "medication" => @"Bạn là dược sĩ. Phân tích thông tin thuốc và trả về JSON với format:
{
  ""summary"": ""Tóm tắt thông tin thuốc"",
  ""medicationName"": ""tên thuốc"",
  ""dosage"": ""liều lượng"",
  ""instructions"": [""hướng dẫn 1"", ""hướng dẫn 2""],
  ""sideEffects"": [""tác dụng phụ nếu có""],
  ""warnings"": [""cảnh báo quan trọng""],
  ""disclaimer"": ""Lưu ý: Tuân thủ chỉ định bác sĩ""
}",
                "lab_value" => @"Bạn là chuyên gia xét nghiệm. Phân tích kết quả xét nghiệm và trả về JSON với format:
{
  ""summary"": ""Tóm tắt kết quả xét nghiệm"",
  ""keyValues"": [""giá trị quan trọng 1"", ""giá trị quan trọng 2""],
  ""normalRanges"": [""khoảng bình thường 1"", ""khoảng bình thường 2""],
  ""abnormalities"": [""bất thường nếu có""],
  ""recommendations"": [""khuyến nghị 1"", ""khuyến nghị 2""],
  ""disclaimer"": ""Lưu ý: Tham khảo ý kiến bác sĩ""
}",
                _ => @"Bạn là trợ lý AI thông minh. Phân tích thông tin và trả về JSON với format:
{
  ""summary"": ""Tóm tắt nội dung"",
  ""keyPoints"": [""điểm chính 1"", ""điểm chính 2""],
  ""insights"": [""insight 1"", ""insight 2""],
  ""recommendations"": [""khuyến nghị 1"", ""khuyến nghị 2""]
}"
            };
        }

        private static string GetInsightPrompt(string dataType)
        {
            return dataType switch
            {
                "nutrition" => "Chỉ phân tích dữ liệu có sẵn. Không thêm thông tin không có trong dữ liệu. Trả về JSON theo format đã chỉ định:",
                "medical" => "Chỉ phân tích dữ liệu có sẵn. Không thêm thông tin không có trong dữ liệu. Trả về JSON theo format đã chỉ định:",
                "medication" => "Chỉ phân tích dữ liệu có sẵn. Không thêm thông tin không có trong dữ liệu. Trả về JSON theo format đã chỉ định:",
                "lab_value" => "Chỉ phân tích dữ liệu có sẵn. Không thêm thông tin không có trong dữ liệu. Trả về JSON theo format đã chỉ định:",
                _ => "Chỉ phân tích dữ liệu có sẵn. Không thêm thông tin không có trong dữ liệu. Trả về JSON theo format đã chỉ định:"
            };
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

    // Response models for OpenAI API
    public class OpenAIResponse
    {
        public Choice[]? Choices { get; set; }
    }

    public class Choice
    {
        public Message? Message { get; set; }
    }

    public class Message
    {
        public string? Content { get; set; }
    }
}

