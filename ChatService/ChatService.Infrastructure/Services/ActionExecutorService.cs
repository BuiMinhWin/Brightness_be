using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.Services
{
    public class ActionExecutorService : IActionExecutorService
    {
        private readonly IPersonalCoachApiService _personalCoachApiService;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<ActionExecutorService> _logger;

        public ActionExecutorService(
            IPersonalCoachApiService personalCoachApiService,
            IGeminiService geminiService,
            ILogger<ActionExecutorService> logger)
        {
            _personalCoachApiService = personalCoachApiService;
            _geminiService = geminiService;
            _logger = logger;
        }

        public async Task<ActionResult> ExecuteActionAsync(Guid userId, IntentDetectionResult intent, UserContextDTO userContext)
        {
            try
            {
                _logger.LogInformation("Executing action {IntentType} for user {UserId}", intent.Type, userId);

                return intent.Type switch
                {
                    // Query operations
                    IntentType.QuerySchedule => await ExecuteQueryScheduleAsync(userId, intent.ExtractedParams),
                    IntentType.QueryProfile => await ExecuteQueryProfileAsync(userId),
                    IntentType.QueryHealthData => await ExecuteQueryHealthDataAsync(userId),
                    IntentType.QueryProgress => await ExecuteQueryProgressAsync(userId),

                    // Create operations
                    IntentType.CreateSchedule => await ExecuteCreateScheduleAsync(userId, intent.ExtractedParams),
                    IntentType.CreateWorkout => await ExecuteCreateWorkoutAsync(userId, intent.ExtractedParams),

                    // Update operations
                    IntentType.UpdatePreference => await ExecuteUpdatePreferenceAsync(userId, intent.ExtractedParams),
                    IntentType.UpdateProfile => await ExecuteUpdateProfileAsync(userId, intent.ExtractedParams),
                    IntentType.UpdateSchedule => await ExecuteUpdateScheduleAsync(userId, intent.ExtractedParams),

                    // Delete operations
                    IntentType.DeleteSchedule => await ExecuteDeleteScheduleAsync(userId, intent.ExtractedParams),
                    IntentType.DeleteHealthData => await ExecuteDeleteHealthDataAsync(userId, intent.ExtractedParams),

                    // Analysis operations
                    IntentType.AnalyzeProgress => await ExecuteAnalyzeProgressAsync(userId, userContext),
                    IntentType.AnalyzeNutrition => await ExecuteAnalyzeNutritionAsync(userId),
                    IntentType.GetRecommendations => await ExecuteGetRecommendationsAsync(userId, userContext),

                    _ => new ActionResult
                    {
                        Success = false,
                        Message = "I'm not sure how to handle that action yet. Could you rephrase or try something else?",
                        ErrorCode = "UNSUPPORTED_ACTION"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action {IntentType} for user {UserId}", intent.Type, userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I encountered an error while processing your request. Please try again.",
                    ErrorCode = "EXECUTION_ERROR"
                };
            }
        }

        #region Query Operations

        private async Task<ActionResult> ExecuteQueryScheduleAsync(Guid userId, Dictionary<string, string> parameters)
        {
            try
            {
                var schedules = await _personalCoachApiService.GetSchedulesAsync(userId);
                
                if (schedules == null || !schedules.Any())
                {
                    return new ActionResult
                    {
                        Success = true,
                        Message = "You don't have any workout schedules yet. Would you like me to create one for you?",
                        Data = new { hasSchedules = false }
                    };
                }

                // Filter by parameters if specified
                var scheduleType = parameters.GetValueOrDefault("scheduleType", "current");
                var timeframe = parameters.GetValueOrDefault("timeframe", "");

                var filteredSchedules = FilterSchedules(schedules, scheduleType, timeframe);

                // Format schedule data conversationally
                var formattedData = FormatScheduleData(filteredSchedules);

                return new ActionResult
                {
                    Success = true,
                    Message = formattedData,
                    Data = new { schedules = filteredSchedules, count = filteredSchedules.Count }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying schedules for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble retrieving your schedules. Please try again.",
                    ErrorCode = "QUERY_SCHEDULE_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteQueryProfileAsync(Guid userId)
        {
            try
            {
                var profile = await _personalCoachApiService.GetUserProfileAsync(userId);
                
                if (profile == null)
                {
                    return new ActionResult
                    {
                        Success = true,
                        Message = "I don't have your profile information yet. Would you like to set it up?",
                        Data = new { hasProfile = false }
                    };
                }

                var formattedProfile = FormatProfileData(profile);

                return new ActionResult
                {
                    Success = true,
                    Message = formattedProfile,
                    Data = profile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying profile for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble retrieving your profile. Please try again.",
                    ErrorCode = "QUERY_PROFILE_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteQueryHealthDataAsync(Guid userId)
        {
            try
            {
                var healthData = await _personalCoachApiService.GetHealthDataHistoryAsync(userId);
                
                if (healthData == null || !healthData.Any())
                {
                    return new ActionResult
                    {
                        Success = true,
                        Message = "You haven't uploaded any health data yet. You can upload lab results or body composition data through the Health Data section.",
                        Data = new { hasHealthData = false }
                    };
                }

                var formattedData = FormatHealthData(healthData);

                return new ActionResult
                {
                    Success = true,
                    Message = formattedData,
                    Data = new { healthRecords = healthData, count = healthData.Count }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying health data for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble retrieving your health data. Please try again.",
                    ErrorCode = "QUERY_HEALTH_DATA_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteQueryProgressAsync(Guid userId)
        {
            try
            {
                var progress = await _personalCoachApiService.GetProgressSummaryAsync(userId);
                
                if (progress == null)
                {
                    return new ActionResult
                    {
                        Success = true,
                        Message = "I don't have enough data to show your progress yet. Complete a few workouts and I'll be able to track your improvement!",
                        Data = new { hasProgress = false }
                    };
                }

                var formattedProgress = FormatProgressData(progress);

                return new ActionResult
                {
                    Success = true,
                    Message = formattedProgress,
                    Data = progress
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying progress for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble retrieving your progress data. Please try again.",
                    ErrorCode = "QUERY_PROGRESS_ERROR"
                };
            }
        }

        #endregion

        #region Create Operations

        private async Task<ActionResult> ExecuteCreateScheduleAsync(Guid userId, Dictionary<string, string> parameters)
        {
            try
            {
                // Build schedule request from parameters
                var goal = parameters.GetValueOrDefault("goal", "weight_loss");
                var difficulty = parameters.GetValueOrDefault("difficulty", "intermediate");
                var duration = int.Parse(parameters.GetValueOrDefault("duration", "28"));

                var scheduleRequest = new ChatService.Application.DTOs.CreateScheduleFromChatRequest
                {
                    SessionId = Guid.NewGuid(), // Temp session for action-based creation
                    Goal = goal,
                    DifficultyLevel = difficulty,
                    DurationDays = duration,
                    AdditionalNotes = ""
                };

                var schedule = await _personalCoachApiService.CreateScheduleAsync(userId, scheduleRequest);

                return new ActionResult
                {
                    Success = true,
                    Message = $"Great! I've created a {duration}-day {difficulty} {goal} workout schedule for you. Your schedule includes targeted exercises and starts today. You can view it in your Schedule section.",
                    Data = schedule,
                    CreditsUsed = 10
                };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient") || ex.Message.Contains("credits"))
            {
                return new ActionResult
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "INSUFFICIENT_CREDITS"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble creating your schedule. Please try again or check if you have enough AI credits.",
                    ErrorCode = "CREATE_SCHEDULE_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteCreateWorkoutAsync(Guid userId, Dictionary<string, string> parameters)
        {
            // Placeholder for workout logging functionality
            return new ActionResult
            {
                Success = false,
                Message = "Workout logging is coming soon! For now, you can track your workouts in the Schedule section.",
                ErrorCode = "NOT_IMPLEMENTED"
            };
        }

        #endregion

        #region Update Operations

        private async Task<ActionResult> ExecuteUpdatePreferenceAsync(Guid userId, Dictionary<string, string> parameters)
        {
            try
            {
                var updates = new Dictionary<string, object>();
                
                if (parameters.ContainsKey("preferredWorkoutTime"))
                    updates["preferredWorkoutTime"] = parameters["preferredWorkoutTime"];
                
                if (parameters.ContainsKey("workoutDaysPerWeek"))
                    updates["workoutDaysPerWeek"] = int.Parse(parameters["workoutDaysPerWeek"]);

                await _personalCoachApiService.UpdateUserPreferencesAsync(userId, updates);

                var message = BuildUpdateMessage("preferences", parameters);

                return new ActionResult
                {
                    Success = true,
                    Message = message,
                    Data = updates
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preferences for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble updating your preferences. Please try again.",
                    ErrorCode = "UPDATE_PREFERENCE_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteUpdateProfileAsync(Guid userId, Dictionary<string, string> parameters)
        {
            try
            {
                var updates = new Dictionary<string, object>();
                
                foreach (var param in parameters)
                {
                    if (param.Key == "height" || param.Key == "weight")
                        updates[param.Key] = double.Parse(param.Value);
                    else if (param.Key == "age")
                        updates[param.Key] = int.Parse(param.Value);
                    else
                        updates[param.Key] = param.Value;
                }

                await _personalCoachApiService.UpdateUserProfileAsync(userId, updates);

                var message = BuildUpdateMessage("profile", parameters);

                return new ActionResult
                {
                    Success = true,
                    Message = message,
                    Data = updates
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble updating your profile. Please try again.",
                    ErrorCode = "UPDATE_PROFILE_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteUpdateScheduleAsync(Guid userId, Dictionary<string, string> parameters)
        {
            // Placeholder - schedule updates need schedule ID
            return new ActionResult
            {
                Success = false,
                Message = "To update a specific schedule, please go to the Schedule section and use the edit options there. Or I can create a new schedule for you!",
                ErrorCode = "NOT_FULLY_IMPLEMENTED"
            };
        }

        #endregion

        #region Delete Operations

        private async Task<ActionResult> ExecuteDeleteScheduleAsync(Guid userId, Dictionary<string, string> parameters)
        {
            try
            {
                var scheduleType = parameters.GetValueOrDefault("scheduleType", "current");
                
                // For now, guide user to UI for deletion
                return new ActionResult
                {
                    Success = false,
                    Message = "To delete a schedule, please go to the Schedule section and use the delete option there. This ensures you don't accidentally remove important data.",
                    ErrorCode = "REQUIRES_UI_CONFIRMATION"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble processing your delete request. Please use the Schedule section to manage your schedules.",
                    ErrorCode = "DELETE_SCHEDULE_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteDeleteHealthDataAsync(Guid userId, Dictionary<string, string> parameters)
        {
            return new ActionResult
            {
                Success = false,
                Message = "To delete health data, please go to the Health Data section. This ensures your important health records are protected from accidental deletion.",
                ErrorCode = "REQUIRES_UI_CONFIRMATION"
            };
        }

        #endregion

        #region Analysis Operations

        private async Task<ActionResult> ExecuteAnalyzeProgressAsync(Guid userId, UserContextDTO userContext)
        {
            try
            {
                var progress = await _personalCoachApiService.GetProgressSummaryAsync(userId);
                
                if (progress == null)
                {
                    return new ActionResult
                    {
                        Success = true,
                        Message = "I don't have enough workout data to analyze your progress yet. Complete a few more workouts and I'll be able to give you insights!",
                        Data = new { hasProgress = false }
                    };
                }

                // Use Gemini to analyze progress
                var analysisPrompt = $"Analyze this user's fitness progress and provide insights:\n\n{System.Text.Json.JsonSerializer.Serialize(progress)}\n\nProvide encouraging feedback, highlight improvements, and suggest areas to focus on.";
                
                var analysis = await _geminiService.GenerateResponseAsync(analysisPrompt, userContext);

                return new ActionResult
                {
                    Success = true,
                    Message = analysis,
                    Data = progress
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing progress for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble analyzing your progress. Please try again.",
                    ErrorCode = "ANALYZE_PROGRESS_ERROR"
                };
            }
        }

        private async Task<ActionResult> ExecuteAnalyzeNutritionAsync(Guid userId)
        {
            return new ActionResult
            {
                Success = true,
                Message = "Nutrition tracking and analysis is coming soon! For now, focus on your workout routine and I can help track your fitness progress.",
                Data = new { feature = "coming_soon" }
            };
        }

        private async Task<ActionResult> ExecuteGetRecommendationsAsync(Guid userId, UserContextDTO userContext)
        {
            try
            {
                // Get user's current data
                var schedules = await _personalCoachApiService.GetSchedulesAsync(userId);
                var profile = await _personalCoachApiService.GetUserProfileAsync(userId);

                // Use Gemini to generate personalized recommendations
                var recommendationPrompt = $"Based on this user's profile and workout history, provide 3-5 specific, actionable recommendations to help them achieve their fitness goals:\n\nProfile: {System.Text.Json.JsonSerializer.Serialize(profile)}\nSchedules: {System.Text.Json.JsonSerializer.Serialize(schedules)}\n\nUser Context: {userContext}";
                
                var recommendations = await _geminiService.GenerateResponseAsync(recommendationPrompt, userContext);

                return new ActionResult
                {
                    Success = true,
                    Message = recommendations,
                    Data = new { profile, hasSchedules = schedules?.Any() ?? false }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations for user {UserId}", userId);
                return new ActionResult
                {
                    Success = false,
                    Message = "I had trouble generating recommendations. Please try again.",
                    ErrorCode = "RECOMMENDATIONS_ERROR"
                };
            }
        }

        #endregion

        #region Helper Methods

        private List<object> FilterSchedules(List<object> schedules, string scheduleType, string timeframe)
        {
            // Simple filtering logic - can be enhanced based on actual schedule structure
            return schedules;
        }

        private string FormatScheduleData(List<object> schedules)
        {
            if (!schedules.Any())
                return "You don't have any schedules matching those criteria.";

            var sb = new StringBuilder();
            sb.AppendLine($"I found {schedules.Count} workout schedule(s) for you:");
            sb.AppendLine();

            foreach (var schedule in schedules.Take(3)) // Show first 3
            {
                sb.AppendLine($"- {System.Text.Json.JsonSerializer.Serialize(schedule)}");
            }

            if (schedules.Count > 3)
                sb.AppendLine($"\n...and {schedules.Count - 3} more. View all in your Schedule section.");

            return sb.ToString();
        }

        private string FormatProfileData(object profile)
        {
            return $"Here's your profile information:\n\n{System.Text.Json.JsonSerializer.Serialize(profile, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}";
        }

        private string FormatHealthData(List<object> healthData)
        {
            return $"I found {healthData.Count} health record(s) for you. You can view detailed information in the Health Data section.";
        }

        private string FormatProgressData(object progress)
        {
            return $"Here's your progress summary:\n\n{System.Text.Json.JsonSerializer.Serialize(progress, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}";
        }

        private string BuildUpdateMessage(string type, Dictionary<string, string> parameters)
        {
            var updates = string.Join(", ", parameters.Select(p => $"{p.Key}: {p.Value}"));
            return $"Perfect! I've updated your {type}: {updates}. These changes will be reflected in your workout recommendations.";
        }

        #endregion
    }
}

