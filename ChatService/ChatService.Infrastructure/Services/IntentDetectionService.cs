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
        // Create operations
        private static readonly string[] ScheduleCreationKeywords = {
            "create schedule", "make workout plan", "generate exercises", "build a plan",
            "workout schedule", "training plan", "exercise program", "new schedule"
        };

        private static readonly string[] WorkoutCreationKeywords = {
            "create workout", "add workout", "log workout", "new workout"
        };

        // Read operations
        private static readonly string[] ScheduleQueryKeywords = {
            "show my schedule", "what's my workout", "my training plan", "view schedule",
            "today's workout", "current plan", "see my schedule", "display schedule",
            "get my schedule", "my schedules"
        };

        private static readonly string[] ProfileQueryKeywords = {
            "show my profile", "what's my profile", "view my profile", "my profile",
            "see my profile", "display profile", "get my profile", "my info",
            "my information", "about me"
        };

        private static readonly string[] HealthDataQueryKeywords = {
            "show my health data", "view health records", "my health data",
            "health history", "my measurements", "body composition"
        };

        private static readonly string[] ProgressQueryKeywords = {
            "show my progress", "view progress", "my progress", "how am i doing",
            "track progress", "progress report"
        };

        // Update operations
        private static readonly string[] PreferenceUpdateKeywords = {
            "i prefer", "i like", "i want to workout", "change my preference",
            "update my settings", "i can only workout", "change settings",
            "modify preferences"
        };

        private static readonly string[] ProfileUpdateKeywords = {
            "update my profile", "change my", "edit my profile", "modify profile",
            "update height", "update weight", "change age"
        };

        private static readonly string[] ScheduleUpdateKeywords = {
            "update schedule", "modify schedule", "change schedule", "edit schedule",
            "adjust schedule"
        };

        // Delete operations
        private static readonly string[] ScheduleDeleteKeywords = {
            "delete schedule", "remove schedule", "cancel schedule", "delete my schedule",
            "remove my plan"
        };

        private static readonly string[] HealthDataDeleteKeywords = {
            "delete health data", "remove health record", "delete measurement"
        };

        // Analysis operations
        private static readonly string[] ProgressAnalysisKeywords = {
            "analyze my progress", "how am i progressing", "am i improving",
            "track my improvement", "analyze performance"
        };

        private static readonly string[] NutritionAnalysisKeywords = {
            "analyze nutrition", "diet analysis", "nutrition advice", "meal plan"
        };

        private static readonly string[] RecommendationKeywords = {
            "recommend", "suggest", "what should i", "advice", "tips",
            "help me improve"
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
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
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
                    IsSafe = false,
                    Risk = RiskLevel.High,
                    RequiresConfirmation = false
                });
            }

            // Delete operations - High risk, always confirm
            if (ContainsAny(messageLower, ScheduleDeleteKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.DeleteSchedule,
                    IsSafe = true,
                    Risk = RiskLevel.High,
                    RequiresConfirmation = true,
                    ExtractedParams = ExtractScheduleIdentifier(messageLower),
                    SuggestedAction = "Delete your workout schedule"
                });
            }

            if (ContainsAny(messageLower, HealthDataDeleteKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.DeleteHealthData,
                    IsSafe = true,
                    Risk = RiskLevel.High,
                    RequiresConfirmation = true,
                    SuggestedAction = "Delete health data record"
                });
            }

            // Update operations - Medium risk, always confirm
            if (ContainsAny(messageLower, ScheduleUpdateKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.UpdateSchedule,
                    IsSafe = true,
                    Risk = RiskLevel.Medium,
                    RequiresConfirmation = true,
                    ExtractedParams = ExtractScheduleParams(messageLower),
                    SuggestedAction = "Update your workout schedule"
                });
            }

            if (ContainsAny(messageLower, ProfileUpdateKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.UpdateProfile,
                    IsSafe = true,
                    Risk = RiskLevel.Medium,
                    RequiresConfirmation = true,
                    ExtractedParams = ExtractProfileParams(messageLower),
                    SuggestedAction = "Update your profile"
                });
            }

            if (ContainsAny(messageLower, PreferenceUpdateKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.UpdatePreference,
                    IsSafe = true,
                    Risk = RiskLevel.Medium,
                    RequiresConfirmation = true,
                    ExtractedParams = ExtractPreferenceParams(messageLower),
                    SuggestedAction = "Update your workout preferences"
                });
            }

            // Create operations - Low risk, confirm if ambiguous
            if (ContainsAny(messageLower, ScheduleCreationKeywords))
            {
                var scheduleParams = ExtractScheduleParams(messageLower);
                var isAmbiguous = !scheduleParams.ContainsKey("goal") || !scheduleParams.ContainsKey("difficulty");
                
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.CreateSchedule,
                    IsSafe = true,
                    Risk = RiskLevel.Low,
                    RequiresConfirmation = isAmbiguous,
                    ExtractedParams = scheduleParams,
                    SuggestedAction = "Create a new workout schedule"
                });
            }

            if (ContainsAny(messageLower, WorkoutCreationKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.CreateWorkout,
                    IsSafe = true,
                    Risk = RiskLevel.Low,
                    RequiresConfirmation = false,
                    SuggestedAction = "Log a workout session"
                });
            }

            // Read operations - Safe, no confirmation
            if (ContainsAny(messageLower, ScheduleQueryKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.QuerySchedule,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false,
                    ExtractedParams = ExtractScheduleIdentifier(messageLower)
                });
            }

            if (ContainsAny(messageLower, ProfileQueryKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.QueryProfile,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
                });
            }

            if (ContainsAny(messageLower, HealthDataQueryKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.QueryHealthData,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
                });
            }

            if (ContainsAny(messageLower, ProgressQueryKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.QueryProgress,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
                });
            }

            // Analysis operations - Safe, no confirmation
            if (ContainsAny(messageLower, ProgressAnalysisKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.AnalyzeProgress,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
                });
            }

            if (ContainsAny(messageLower, NutritionAnalysisKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.AnalyzeNutrition,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
                });
            }

            if (ContainsAny(messageLower, RecommendationKeywords))
            {
                return Task.FromResult(new IntentDetectionResult
                {
                    Type = IntentType.GetRecommendations,
                    IsSafe = true,
                    Risk = RiskLevel.Safe,
                    RequiresConfirmation = false
                });
            }

            // Default to normal conversation
            return Task.FromResult(new IntentDetectionResult
            {
                Type = IntentType.Normal,
                IsSafe = true,
                Risk = RiskLevel.Safe,
                RequiresConfirmation = false
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

        private Dictionary<string, string> ExtractScheduleIdentifier(string message)
        {
            var parameters = new Dictionary<string, string>();

            // Extract schedule references
            if (message.Contains("current") || message.Contains("active") || message.Contains("my schedule"))
                parameters["scheduleType"] = "current";
            else if (message.Contains("latest") || message.Contains("recent"))
                parameters["scheduleType"] = "latest";
            else if (message.Contains("all") || message.Contains("my schedules"))
                parameters["scheduleType"] = "all";
            
            // Extract time references
            if (message.Contains("today"))
                parameters["timeframe"] = "today";
            else if (message.Contains("this week"))
                parameters["timeframe"] = "week";
            else if (message.Contains("this month"))
                parameters["timeframe"] = "month";

            return parameters;
        }

        private Dictionary<string, string> ExtractProfileParams(string message)
        {
            var parameters = new Dictionary<string, string>();

            // Extract height updates
            var heightMatch = Regex.Match(message, @"height.*?(\d+\.?\d*)\s*(cm|m|ft|feet|inches?)?");
            if (heightMatch.Success)
            {
                parameters["height"] = heightMatch.Groups[1].Value;
                if (heightMatch.Groups[2].Success)
                    parameters["heightUnit"] = heightMatch.Groups[2].Value;
            }

            // Extract weight updates
            var weightMatch = Regex.Match(message, @"weight.*?(\d+\.?\d*)\s*(kg|lbs?|pounds?)?");
            if (weightMatch.Success)
            {
                parameters["weight"] = weightMatch.Groups[1].Value;
                if (weightMatch.Groups[2].Success)
                    parameters["weightUnit"] = weightMatch.Groups[2].Value;
            }

            // Extract age updates
            var ageMatch = Regex.Match(message, @"age.*?(\d+)");
            if (ageMatch.Success)
            {
                parameters["age"] = ageMatch.Groups[1].Value;
            }

            // Extract gender
            if (message.Contains("male") && !message.Contains("female"))
                parameters["gender"] = "male";
            else if (message.Contains("female"))
                parameters["gender"] = "female";

            return parameters;
        }
    }
}

