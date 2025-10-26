using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace ChatService.Infrastructure.Services
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

        public async Task<object> CreateScheduleAsync(Guid userId, CreateScheduleFromChatRequest request)
        {
            try
            {
                // Get user preferences for available days and equipment
                var userPreferences = await GetUserPreferencesAsync(userId);
                var availableDays = ParseAvailableDays(userPreferences?.AvailableDays);
                var equipmentAvailable = ParseEquipmentAvailable(userPreferences?.EquipmentAvailable);

                var scheduleRequest = new
                {
                    UserId = userId,
                    Goal = request.Goal,
                    DurationDays = request.DurationDays,
                    DifficultyLevel = request.DifficultyLevel,
                    AdditionalNotes = request.AdditionalNotes,
                    AvailableDays = availableDays,
                    EquipmentAvailable = equipmentAvailable,
                    StartFromToday = true
                };

                var json = JsonConvert.SerializeObject(scheduleRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_personalCoachApiUrl}/api/Schedule/generate");
                httpRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create schedule for user {UserId}: {StatusCode} - {Error}", 
                        userId, response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to create schedule: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Successfully created schedule for user {UserId}", userId);
                _logger.LogDebug("Schedule response: {Response}", responseContent);
                
                // Parse the JSON and extract the properties explicitly to avoid serialization issues
                var responseJson = JObject.Parse(responseContent);
                
                return new
                {
                    scheduleId = responseJson["scheduleId"]?.ToString(),
                    scheduleName = responseJson["scheduleName"]?.ToString(),
                    goal = responseJson["goal"]?.ToString(),
                    difficultyLevel = responseJson["difficultyLevel"]?.ToString(),
                    durationDays = responseJson["durationDays"]?.ToObject<int?>(),
                    days = responseJson["days"],
                    createdAt = responseJson["createdAt"]?.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<object>> GetUserSchedulesAsync(Guid userId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_personalCoachApiUrl}/api/Schedule/user/{userId}");
                httpRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get schedules for user {UserId}: {StatusCode}", 
                        userId, response.StatusCode);
                    return new List<object>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<object>>(responseContent) ?? new List<object>();

                _logger.LogInformation("Retrieved {Count} schedules for user {UserId}", result.Count, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedules for user {UserId}", userId);
                return new List<object>();
            }
        }

        public async Task UpdateUserPreferenceAsync(Guid userId, PreferenceUpdateDTO preference)
        {
            try
            {
                // First, get the current user profile
                var getCurrentRequest = new HttpRequestMessage(HttpMethod.Get, $"{_personalCoachApiUrl}/api/UserProfile/{userId}");
                getCurrentRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);
                
                var currentResponse = await _httpClient.SendAsync(getCurrentRequest);
                object currentProfile = null;
                
                if (currentResponse.IsSuccessStatusCode)
                {
                    var currentContent = await currentResponse.Content.ReadAsStringAsync();
                    currentProfile = JsonConvert.DeserializeObject<object>(currentContent);
                }

                // Prepare update request - merge with current profile or use defaults
                var updateRequest = new
                {
                    UserId = userId,
                    HeightCm = 170,  // Default
                    CurrentWeightKg = 70,  // Default
                    TargetWeightKg = 70,  // Default
                    FitnessLevel = "intermediate",
                    FitnessGoal = "general_fitness",
                    PreferredWorkoutTime = preference.PreferredWorkoutTime ?? "morning",
                    WorkoutDurationMinutes = 60,
                    AvailableDays = new[] { "Monday", "Wednesday", "Friday" },  // Will be updated based on WorkoutDaysPerWeek if provided
                    EquipmentAvailable = new[] { "Dumbbells", "Bodyweight" },
                    GymAccess = false,
                    HomeWorkoutOnly = true,
                    DietaryRestrictions = new string[] { },
                    DislikedExercises = new string[] { },
                    FavoriteExercises = new string[] { }
                };

                var jsonRequest = JsonConvert.SerializeObject(updateRequest);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_personalCoachApiUrl}/api/UserProfile");
                httpRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to update preferences for user {UserId}: {StatusCode} - {Error}", 
                        userId, response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to update preferences: {response.StatusCode} - {errorContent}");
                }

                _logger.LogInformation("Successfully updated preferences for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preferences for user {UserId}", userId);
                throw;
            }
        }

        private async Task<UserPreference?> GetUserPreferencesAsync(Guid userId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_personalCoachApiUrl}/api/UserProfile/{userId}/preferences");
                httpRequest.Headers.Add("X-Internal-Api-Key", _internalApiKey);

                var response = await _httpClient.SendAsync(httpRequest);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserPreference>(content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user preferences for {UserId}, using defaults", userId);
            }
            return null;
        }

        private List<string> ParseAvailableDays(string? availableDaysJson)
        {
            if (string.IsNullOrWhiteSpace(availableDaysJson))
            {
                // Default to weekdays if no preferences
                return new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            }

            try
            {
                var days = JsonConvert.DeserializeObject<string[]>(availableDaysJson);
                return days?.ToList() ?? new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            }
            catch
            {
                return new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            }
        }

        private List<string> ParseEquipmentAvailable(string? equipmentJson)
        {
            if (string.IsNullOrWhiteSpace(equipmentJson))
            {
                // Default to basic equipment
                return new List<string> { "Bodyweight", "None" };
            }

            try
            {
                var equipment = JsonConvert.DeserializeObject<string[]>(equipmentJson);
                return equipment?.ToList() ?? new List<string> { "Bodyweight", "None" };
            }
            catch
            {
                return new List<string> { "Bodyweight", "None" };
            }
        }
    }

    public class UserPreference
    {
        public string? AvailableDays { get; set; }
        public string? EquipmentAvailable { get; set; }
    }
}

