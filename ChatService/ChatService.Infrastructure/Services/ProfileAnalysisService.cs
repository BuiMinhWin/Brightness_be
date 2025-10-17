using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ChatService.Infrastructure.Services
{
    public class ProfileAnalysisService : IProfileAnalysisService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ProfileAnalysisService> _logger;

        public ProfileAnalysisService(ChatDbContext context, ILogger<ProfileAnalysisService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ScheduleRecommendationDTO> AnalyzeUserProfileAsync(Guid userId)
        {
            try
            {
                // Fetch user profile data
                var userProfile = await _context.UserProfiles
                    .Where(p => p.UserId == userId)
                    .FirstOrDefaultAsync();

                var userPreferences = await _context.UserPreferences
                    .Where(p => p.UserId == userId)
                    .FirstOrDefaultAsync();

                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                // Build reasoning
                var reasoning = new StringBuilder();

                // Determine Goal
                string goal = DetermineGoal(userProfile, reasoning);

                // Determine Difficulty Level
                string difficultyLevel = DetermineDifficultyLevel(userProfile, reasoning);

                // Determine Duration
                int durationDays = DetermineDuration(userPreferences, reasoning);

                _logger.LogInformation(
                    "Profile analysis for user {UserId}: Goal={Goal}, Difficulty={Difficulty}, Duration={Duration}",
                    userId, goal, difficultyLevel, durationDays);

                return new ScheduleRecommendationDTO(
                    goal,
                    difficultyLevel,
                    durationDays,
                    reasoning.ToString().Trim()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing profile for user {UserId}", userId);
                
                // Return safe defaults
                return new ScheduleRecommendationDTO(
                    "weight_loss",
                    "intermediate",
                    30,
                    "Using default recommendations. Complete your fitness profile for personalized plans."
                );
            }
        }

        private string DetermineGoal(Domain.Entities.UserProfile? profile, StringBuilder reasoning)
        {
            if (profile == null)
            {
                reasoning.Append("No fitness profile found. ");
                return "weight_loss";
            }

            var fitnessGoal = profile.FitnessGoal?.ToLowerInvariant() ?? "";

            if (fitnessGoal.Contains("muscle") || fitnessGoal.Contains("gain") || fitnessGoal.Contains("bulk"))
            {
                reasoning.Append($"Muscle gain goal detected. ");
                return "muscle_gain";
            }
            else if (fitnessGoal.Contains("weight") || fitnessGoal.Contains("lose") || fitnessGoal.Contains("fat"))
            {
                reasoning.Append($"Weight loss goal detected. ");
                return "weight_loss";
            }
            else if (fitnessGoal.Contains("strength") || fitnessGoal.Contains("power"))
            {
                reasoning.Append($"Strength training goal detected. ");
                return "strength";
            }
            else if (fitnessGoal.Contains("endurance") || fitnessGoal.Contains("cardio") || fitnessGoal.Contains("stamina"))
            {
                reasoning.Append($"Endurance goal detected. ");
                return "endurance";
            }
            else if (fitnessGoal.Contains("flexibility") || fitnessGoal.Contains("mobility"))
            {
                reasoning.Append($"Flexibility goal detected. ");
                return "flexibility";
            }

            // Check weight difference for implicit goal
            if (profile.CurrentWeightKg > profile.TargetWeightKg + 5)
            {
                reasoning.Append($"Weight loss inferred (current: {profile.CurrentWeightKg}kg, target: {profile.TargetWeightKg}kg). ");
                return "weight_loss";
            }
            else if (profile.TargetWeightKg > profile.CurrentWeightKg + 3)
            {
                reasoning.Append($"Muscle gain inferred (current: {profile.CurrentWeightKg}kg, target: {profile.TargetWeightKg}kg). ");
                return "muscle_gain";
            }

            reasoning.Append("General fitness goal. ");
            return "weight_loss"; // Default
        }

        private string DetermineDifficultyLevel(Domain.Entities.UserProfile? profile, StringBuilder reasoning)
        {
            if (profile == null)
            {
                return "intermediate";
            }

            var fitnessLevel = profile.FitnessLevel?.ToLowerInvariant() ?? "";

            if (fitnessLevel.Contains("beginner") || fitnessLevel.Contains("novice") || fitnessLevel.Contains("starter"))
            {
                reasoning.Append($"{profile.FitnessLevel} fitness level. ");
                return "beginner";
            }
            else if (fitnessLevel.Contains("advanced") || fitnessLevel.Contains("expert") || fitnessLevel.Contains("pro"))
            {
                reasoning.Append($"{profile.FitnessLevel} fitness level. ");
                return "advanced";
            }
            else if (fitnessLevel.Contains("intermediate") || fitnessLevel.Contains("moderate"))
            {
                reasoning.Append($"{profile.FitnessLevel} fitness level. ");
                return "intermediate";
            }

            reasoning.Append("Intermediate level recommended. ");
            return "intermediate";
        }

        private int DetermineDuration(Domain.Entities.UserPreference? preferences, StringBuilder reasoning)
        {
            if (preferences == null)
            {
                return 30; // Default 1 month
            }

            // Parse available days (JSON string like "[\"Monday\",\"Wednesday\",\"Friday\"]")
            int daysPerWeek = 3; // Default
            
            if (!string.IsNullOrWhiteSpace(preferences.AvailableDays))
            {
                try
                {
                    var daysArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(preferences.AvailableDays);
                    daysPerWeek = daysArray?.Length ?? 3;
                }
                catch
                {
                    // If parsing fails, use default
                }
            }

            // Calculate optimal duration based on days per week
            // More days per week = can achieve goals faster
            // Cap at 30 days (1 month) to prevent backend overload
            int weeks;
            if (daysPerWeek >= 5)
            {
                weeks = 4; // 4 weeks for 5+ days/week
                reasoning.Append($"{daysPerWeek} days/week availability - 4 week program. ");
            }
            else if (daysPerWeek >= 3)
            {
                weeks = 4; // 4 weeks for 3-4 days/week (capped from 6 weeks)
                reasoning.Append($"{daysPerWeek} days/week availability - 4 week program. ");
            }
            else
            {
                weeks = 4; // 4 weeks for 1-2 days/week (capped from 8 weeks)
                reasoning.Append($"{daysPerWeek} days/week availability - 4 week program. ");
            }

            int durationDays = weeks * 7;
            
            // Ensure we never exceed 30 days
            if (durationDays > 30)
            {
                durationDays = 30;
                reasoning.Append("Duration capped at 30 days for optimal performance. ");
            }

            return durationDays;
        }
    }
}

