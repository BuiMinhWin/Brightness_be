using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly ChatDbContext _dbContext;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private readonly decimal _tokenPricePer1K;
        private readonly ILogger<GeminiService> _logger;
        private const string SystemInstruction = @"You are BrightNest Assistant, an AI health and fitness coach. Your role is to:

1. Provide personalized health, fitness, and nutrition advice based on user's data
2. Answer questions ONLY related to:
   - Health and wellness
   - Fitness and exercise
   - Nutrition and diet
   - Mental well-being related to fitness
   - Workout planning and techniques
   - Recovery and rest

3. IMPORTANT: If a user asks about topics unrelated to health/fitness (politics, entertainment, general knowledge, etc.), politely decline and redirect them to fitness topics. Example responses:
   - ""I'm specialized in health and fitness topics. I can't help with that, but I'd be happy to discuss your fitness goals or nutrition!""
   - ""That's outside my area of expertise. Let's focus on your health and wellness journey instead!""

4. Use the user's personal context (age, weight, goals, measurements) to personalize your responses
5. Be encouraging, supportive, and professional
6. Keep responses concise but informative
7. Always prioritize user safety - recommend consulting healthcare professionals for medical concerns";

        public GeminiService(
            IConfiguration configuration,
            HttpClient httpClient,
            ChatDbContext dbContext,
            ILogger<GeminiService> logger)
        {
            _apiKey = configuration["GoogleAI:ApiKey"] 
                ?? configuration["Gemini:ApiKey"]
                ?? configuration["GEMINI_API_KEY"]
                ?? configuration["GOOGLE_AI_API_KEY"]
                ?? throw new InvalidOperationException("Gemini API key not configured. Set GoogleAI:ApiKey, GEMINI_API_KEY or GOOGLE_AI_API_KEY in .env");

            _model = configuration["GoogleAI:Model"] ?? "gemini-2.5-flash";
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;

            var priceSetting = configuration["GoogleCloud:TokenPricePer1K"]
                ?? configuration["GOOGLE_CLOUD__TOKEN_PRICE_PER_1K"]
                ?? "0";

            if (!decimal.TryParse(priceSetting, NumberStyles.Float, CultureInfo.InvariantCulture, out _tokenPricePer1K))
            {
                decimal.TryParse(priceSetting, NumberStyles.Float, CultureInfo.CurrentCulture, out _tokenPricePer1K);
            }
        }

        public async Task<string> GenerateResponseAsync(string userMessage, UserContextDTO userContext)
        {
            try
            {
                // Build context from user data
                var contextBuilder = new StringBuilder();
                contextBuilder.AppendLine("User Context:");
                
                if (!string.IsNullOrWhiteSpace(userContext.FullName))
                    contextBuilder.AppendLine($"- Name: {userContext.FullName}");
                
                if (userContext.Age.HasValue)
                    contextBuilder.AppendLine($"- Age: {userContext.Age}");
                
                if (!string.IsNullOrWhiteSpace(userContext.Gender))
                    contextBuilder.AppendLine($"- Gender: {userContext.Gender}");
                
                if (userContext.CurrentWeight.HasValue)
                    contextBuilder.AppendLine($"- Current Weight: {userContext.CurrentWeight}kg");
                
                if (userContext.TargetWeight.HasValue)
                    contextBuilder.AppendLine($"- Target Weight: {userContext.TargetWeight}kg");
                
                if (userContext.Height.HasValue)
                    contextBuilder.AppendLine($"- Height: {userContext.Height}cm");
                
                if (userContext.BodyFat.HasValue)
                    contextBuilder.AppendLine($"- Body Fat: {userContext.BodyFat}%");
                
                if (!string.IsNullOrWhiteSpace(userContext.FitnessLevel))
                    contextBuilder.AppendLine($"- Fitness Level: {userContext.FitnessLevel}");
                
                if (userContext.Goals.Any())
                    contextBuilder.AppendLine($"- Fitness Goals: {string.Join(", ", userContext.Goals)}");
                
                if (!string.IsNullOrWhiteSpace(userContext.PreferredWorkoutTime))
                    contextBuilder.AppendLine($"- Preferred Workout Time: {userContext.PreferredWorkoutTime}");
                
                if (userContext.WorkoutDurationMinutes.HasValue)
                    contextBuilder.AppendLine($"- Preferred Workout Duration: {userContext.WorkoutDurationMinutes} minutes");
                
                if (userContext.GymAccess.HasValue)
                    contextBuilder.AppendLine($"- Gym Access: {(userContext.GymAccess.Value ? "Yes" : "No")}");
                
                if (userContext.HomeWorkoutOnly.HasValue && userContext.HomeWorkoutOnly.Value)
                    contextBuilder.AppendLine("- Preference: Home workouts only");
                
                if (!string.IsNullOrWhiteSpace(userContext.AvailableDays))
                    contextBuilder.AppendLine($"- Available Days: {userContext.AvailableDays}");
                
                if (!string.IsNullOrWhiteSpace(userContext.EquipmentAvailable))
                    contextBuilder.AppendLine($"- Equipment Available: {userContext.EquipmentAvailable}");
                
                if (!string.IsNullOrWhiteSpace(userContext.SubscriptionPlan))
                    contextBuilder.AppendLine($"- Subscription: {userContext.SubscriptionPlan}");
                
                if (!string.IsNullOrWhiteSpace(userContext.ConversationSummary))
                {
                    contextBuilder.AppendLine("\nPrevious Conversation Summary:");
                    contextBuilder.AppendLine(userContext.ConversationSummary);
                }

                contextBuilder.AppendLine($"\nUser Question: {userMessage}");

                // Prepend system instruction to the prompt
                var fullPrompt = $"{SystemInstruction}\n\n{contextBuilder}";
                
                // Use direct HTTP API call to Gemini
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 2048
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

                var generatedText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                await LogUsageAsync(result?.UsageMetadata, "generate_response", userContext);

                return generatedText?.Trim() ?? "I apologize, but I couldn't generate a response. Please try again.";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating AI response: {ex.Message}", ex);
            }
        }

        public async Task<string> SummarizeConversationAsync(string conversationHistory)
        {
            try
            {
                var prompt = $@"Summarize the following conversation history concisely. Focus on:
- Key health/fitness topics discussed
- User's main concerns or questions
- Important context for future conversations
- Any goals or plans mentioned

Keep the summary brief (2-3 sentences maximum) but informative.

Conversation History:
{conversationHistory}

Summary:";

                // Use direct HTTP API call to Gemini
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.3,
                        topK = 32,
                        topP = 1,
                        maxOutputTokens = 500
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

                var summaryText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                await LogUsageAsync(result?.UsageMetadata, "summarize_conversation");

                return summaryText?.Trim() ?? "Unable to generate summary.";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error summarizing conversation: {ex.Message}", ex);
            }
        }

        private async Task LogUsageAsync(UsageMetadata? usageMetadata, string operation, UserContextDTO? userContext = null)
        {
            if (usageMetadata is null)
            {
                return;
            }

            var promptTokens = usageMetadata.PromptTokenCount ?? 0;
            var candidateTokens = usageMetadata.CandidatesTokenCount ?? 0;
            var totalTokens = usageMetadata.TotalTokenCount ?? (promptTokens + candidateTokens);
            var estimatedCost = _tokenPricePer1K > 0
                ? Math.Round((decimal)totalTokens / 1000m * _tokenPricePer1K, 6)
                : 0m;

            var userIdentifier = userContext?.UserId != Guid.Empty ? userContext?.UserId.ToString() : "anonymous";
            var userLabel = !string.IsNullOrWhiteSpace(userContext?.FullName)
                ? $"{userContext?.FullName} ({userIdentifier})"
                : userIdentifier;

            var message =
                $"[Gemini Usage][{operation}] User={userLabel} PromptTokens={promptTokens} OutputTokens={candidateTokens} TotalTokens={totalTokens} EstimatedCost≈{estimatedCost} (per 1k={_tokenPricePer1K})";

            _logger?.LogInformation(message);
            Console.WriteLine(message);

            try
            {
                var log = new AiUsageLog
                {
                    UserId = userContext?.UserId == Guid.Empty ? null : userContext?.UserId,
                    SessionId = null,
                    UserName = userContext?.FullName,
                    Operation = operation,
                    Model = _model,
                    PromptTokens = promptTokens,
                    CompletionTokens = candidateTokens,
                    TotalTokens = totalTokens,
                    EstimatedCost = estimatedCost,
                };

                _dbContext.AiUsageLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to persist AI usage log.");
            }
        }
    }

    // Response models for Gemini API
    public class GeminiResponse
    {
        [JsonProperty("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }

        [JsonProperty("usageMetadata")]
        public UsageMetadata? UsageMetadata { get; set; }
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

    public class UsageMetadata
    {
        [JsonProperty("promptTokenCount")]
        public int? PromptTokenCount { get; set; }

        [JsonProperty("candidatesTokenCount")]
        public int? CandidatesTokenCount { get; set; }

        [JsonProperty("totalTokenCount")]
        public int? TotalTokenCount { get; set; }
    }
}

