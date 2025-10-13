using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;
        private readonly GenerativeModel _model;
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

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"] 
                ?? configuration["GEMINI_API_KEY"]
                ?? configuration["GOOGLE_AI_API_KEY"]
                ?? throw new InvalidOperationException("Gemini API key not configured. Set GEMINI_API_KEY or GOOGLE_AI_API_KEY in .env");

            var googleAI = new GoogleAI(_apiKey);
            _model = googleAI.GenerativeModel(model: "gemini-2.0-flash-exp");
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
                
                if (userContext.Height.HasValue)
                    contextBuilder.AppendLine($"- Height: {userContext.Height}cm");
                
                if (userContext.BodyFat.HasValue)
                    contextBuilder.AppendLine($"- Body Fat: {userContext.BodyFat}%");
                
                if (userContext.Goals.Any())
                    contextBuilder.AppendLine($"- Goals: {string.Join(", ", userContext.Goals)}");
                
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
                
                var response = await _model.GenerateContent(fullPrompt);

                return response?.Text?.Trim() ?? "I apologize, but I couldn't generate a response. Please try again.";
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

                var response = await _model.GenerateContent(prompt);
                return response?.Text?.Trim() ?? "Unable to generate summary.";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error summarizing conversation: {ex.Message}", ex);
            }
        }
    }
}

