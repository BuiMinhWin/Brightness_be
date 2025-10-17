using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ChatService.Infrastructure.Services
{
    public class IntentDetectionService : IIntentDetectionService
    {
        private readonly ILogger<IntentDetectionService> _logger;

        // Blocked patterns for security
        private static readonly string[] AdminKeywords = {
            "admin", "root", "superuser", "grant", "revoke", "drop", "delete", "truncate",
            "privilege", "permission", "role", "authentication", "authorize"
        };

        private static readonly string[] SqlKeywords = {
            "select ", "insert ", "update ", "where ", "union ", "exec(", "execute(",
            "script", "declare ", "create ", "alter ", "database", "table"
        };

        private static readonly string[] SystemCommands = {
            "sudo", "chmod", "rm -rf", "system(", "exec(", "eval(", "bash", "sh ",
            "cmd", "powershell", "process", "kill"
        };

        // Intent detection patterns
        private static readonly string[] ScheduleCreationKeywords = {
            "create schedule", "make workout plan", "generate exercises", "build a plan",
            "workout schedule", "training plan", "exercise program"
        };

        private static readonly string[] PreferenceUpdateKeywords = {
            "i prefer", "i like", "i want to workout", "change my preference",
            "update my settings", "i can only workout"
        };

        private static readonly string[] ScheduleQueryKeywords = {
            "show my schedule", "what's my workout", "my training plan", "view schedule",
            "today's workout", "current plan"
        };

        public IntentDetectionService(ILogger<IntentDetectionService> logger)
        {
            _logger = logger;
        }

        public Task<IntentDetectionResult> DetectIntentAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.Normal,
                    IsSafe = true
                });
            }

            var messageLower = userMessage.ToLowerInvariant();

            // Check for unsafe patterns first
            if (IsUnsafeMessage(messageLower))
            {
                _logger.LogWarning("Unsafe intent detected in message: {Message}", userMessage.Substring(0, Math.Min(100, userMessage.Length)));
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.Unsafe,
                    IsSafe = false
                });
            }

            // Detect specific intents
            if (ContainsAny(messageLower, ScheduleCreationKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.CreateSchedule,
                    IsSafe = true,
                    ExtractedParams = ExtractScheduleParams(messageLower)
                });
            }

            if (ContainsAny(messageLower, PreferenceUpdateKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.UpdatePreference,
                    IsSafe = true,
                    ExtractedParams = ExtractPreferenceParams(messageLower)
                });
            }

            if (ContainsAny(messageLower, ScheduleQueryKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.QuerySchedule,
                    IsSafe = true
                });
            }

            // Default to normal conversation
            return Task.FromResult(new IntentDetectionResult
            {
                Type = IntentType.Normal,
                IsSafe = true
            });
        }

        private bool IsUnsafeMessage(string messageLower)
        {
            // Check for admin keywords
            if (ContainsAny(messageLower, AdminKeywords))
                return true;

            // Check for SQL keywords
            if (ContainsAny(messageLower, SqlKeywords))
                return true;

            // Check for system commands
            if (ContainsAny(messageLower, SystemCommands))
                return true;

            return false;
        }

        private bool ContainsAny(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword));
        }

        private Dictionary<string, string> ExtractScheduleParams(string message)
        {
            var parameters = new Dictionary<string, string>();

            // Extract goal
            if (message.Contains("weight loss") || message.Contains("lose weight"))
                parameters["goal"] = "weight_loss";
            else if (message.Contains("muscle") || message.Contains("gain"))
                parameters["goal"] = "muscle_gain";
            else if (message.Contains("strength"))
                parameters["goal"] = "strength";
            else if (message.Contains("endurance") || message.Contains("cardio"))
                parameters["goal"] = "endurance";
            else if (message.Contains("flexibility") || message.Contains("stretch"))
                parameters["goal"] = "flexibility";

            // Extract difficulty
            if (message.Contains("beginner") || message.Contains("easy") || message.Contains("new to"))
                parameters["difficulty"] = "beginner";
            else if (message.Contains("advanced") || message.Contains("hard") || message.Contains("challenging"))
                parameters["difficulty"] = "advanced";
            else if (message.Contains("intermediate") || message.Contains("moderate"))
                parameters["difficulty"] = "intermediate";

            // Extract duration (simple regex for number + days/weeks)
            var durationMatch = Regex.Match(message, @"(\d+)\s*(day|week|month)s?");
            if (durationMatch.Success)
            {
                var number = int.Parse(durationMatch.Groups[1].Value);
                var unit = durationMatch.Groups[2].Value;
                
                if (unit == "week")
                    parameters["duration"] = (number * 7).ToString();
                else if (unit == "month")
                    parameters["duration"] = (number * 30).ToString();
                else
                    parameters["duration"] = number.ToString();
            }

            return parameters;
        }

        private Dictionary<string, string> ExtractPreferenceParams(string message)
        {
            var parameters = new Dictionary<string, string>();

            // Extract workout time preference
            if (message.Contains("morning"))
                parameters["preferredWorkoutTime"] = "morning";
            else if (message.Contains("afternoon"))
                parameters["preferredWorkoutTime"] = "afternoon";
            else if (message.Contains("evening") || message.Contains("night"))
                parameters["preferredWorkoutTime"] = "evening";

            // Extract days per week
            var daysMatch = Regex.Match(message, @"(\d+)\s*days?\s*(per\s*week|a\s*week)?");
            if (daysMatch.Success)
            {
                parameters["workoutDaysPerWeek"] = daysMatch.Groups[1].Value;
            }

            return parameters;
        }
    }
}

