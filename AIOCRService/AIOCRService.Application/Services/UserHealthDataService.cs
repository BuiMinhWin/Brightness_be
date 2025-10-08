using AIOCRService.Application.DTOs;
using AIOCRService.Application.Interfaces;
using AIOCRService.Application.Helpers;
using AIOCRService.Domain.Entities;
using System.Text;

namespace AIOCRService.Application.Services
{
    public class UserHealthDataService : IUserHealthDataService
    {
        private readonly IOCRRepository _repository;

        public UserHealthDataService(IOCRRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserHealthProfile> GetUserHealthProfileAsync(Guid userId)
        {
            // Get only latest versions by default
            var allData = await _repository.GetProcessedDataByUserIdAsync(userId, latestOnly: true);
            var dataList = allData.ToList();

            var profile = new UserHealthProfile
            {
                UserId = userId
            };

            // Latest body composition
            profile.LatestInBody = dataList
                .Where(d => d.DataType == "lab_result")
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefault()
                ?.ToDTO();

            profile.LatestBloodTest = dataList
                .Where(d => d.DataType == "medical_document" && d.Summary.Contains("blood", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefault()
                ?.ToDTO();

            // Medical info
            profile.InjuryHistory = dataList
                .Where(d => d.DataType == "injury_history")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            profile.AllergiesIntolerances = dataList
                .Where(d => d.DataType == "allergy_test")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            profile.Medications = dataList
                .Where(d => d.DataType == "prescription")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            profile.HealthConditions = dataList
                .Where(d => d.DataType == "health_questionnaire")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            // Fitness data
            profile.FitnessAssessments = dataList
                .Where(d => d.DataType == "fitness_assessment")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            profile.WorkoutLogs = dataList
                .Where(d => d.DataType == "workout_log")
                .OrderByDescending(d => d.CreatedAt)
                .Take(10) // Last 10 workouts
                .Select(d => d.ToDTO())
                .ToList();

            profile.ProgressPhotos = dataList
                .Where(d => d.DataType == "progress_photo")
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .Select(d => d.ToDTO())
                .ToList();

            // Nutrition data
            profile.MealPlans = dataList
                .Where(d => d.DataType == "meal_plan")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            profile.SupplementsUsed = dataList
                .Where(d => d.DataType == "supplement_label")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToDTO())
                .ToList();

            // Build summaries
            profile.HealthSummary = BuildHealthSummary(profile);
            profile.FitnessSummary = BuildFitnessSummary(profile);
            profile.NutritionSummary = BuildNutritionSummary(profile);
            profile.RestrictionsSummary = BuildRestrictionsSummary(profile);

            return profile;
        }

        public async Task<IEnumerable<ProcessedDataDTO>> GetUserDataByTypeAsync(Guid userId, string dataType)
        {
            // Get only latest by default
            var allData = await _repository.GetProcessedDataByUserIdAsync(userId, latestOnly: true);
            return allData.Where(d => d.DataType == dataType).Select(d => d.ToDTO());
        }

        public async Task<string> BuildUserContextForAIAsync(Guid userId)
        {
            var profile = await GetUserHealthProfileAsync(userId);
            
            var context = new StringBuilder();
            context.AppendLine("=== USER HEALTH PROFILE ===");
            context.AppendLine();

            // Body Composition
            if (profile.LatestInBody != null)
            {
                context.AppendLine("BODY COMPOSITION:");
                context.AppendLine(profile.LatestInBody.Summary);
                context.AppendLine();
            }

            // Health Status
            if (profile.HealthSummary.Length > 0)
            {
                context.AppendLine("HEALTH STATUS:");
                context.AppendLine(profile.HealthSummary);
                context.AppendLine();
            }

            // Injuries & Restrictions
            if (profile.InjuryHistory.Any())
            {
                context.AppendLine("INJURY HISTORY:");
                foreach (var injury in profile.InjuryHistory.Take(3))
                {
                    context.AppendLine($"- {injury.Summary}");
                }
                context.AppendLine();
            }

            // Allergies & Intolerances
            if (profile.AllergiesIntolerances.Any())
            {
                context.AppendLine("ALLERGIES/INTOLERANCES:");
                foreach (var allergy in profile.AllergiesIntolerances)
                {
                    context.AppendLine($"- {allergy.Summary}");
                }
                context.AppendLine();
            }

            // Fitness Level
            if (profile.FitnessSummary.Length > 0)
            {
                context.AppendLine("FITNESS LEVEL:");
                context.AppendLine(profile.FitnessSummary);
                context.AppendLine();
            }

            // Nutrition Info
            if (profile.NutritionSummary.Length > 0)
            {
                context.AppendLine("NUTRITION:");
                context.AppendLine(profile.NutritionSummary);
                context.AppendLine();
            }

            // Restrictions Summary
            if (profile.RestrictionsSummary.Length > 0)
            {
                context.AppendLine("RESTRICTIONS:");
                context.AppendLine(profile.RestrictionsSummary);
                context.AppendLine();
            }

            return context.ToString();
        }

        private string BuildHealthSummary(UserHealthProfile profile)
        {
            var summary = new StringBuilder();
            
            if (profile.LatestBloodTest != null)
            {
                summary.AppendLine($"Blood Test: {profile.LatestBloodTest.Summary}");
            }

            if (profile.HealthConditions.Any())
            {
                summary.AppendLine($"Health Conditions: {string.Join(", ", profile.HealthConditions.Select(h => h.Summary))}");
            }

            if (profile.Medications.Any())
            {
                summary.AppendLine($"Medications: {profile.Medications.Count} active");
            }

            return summary.ToString();
        }

        private string BuildFitnessSummary(UserHealthProfile profile)
        {
            var summary = new StringBuilder();
            
            if (profile.FitnessAssessments.Any())
            {
                var latest = profile.FitnessAssessments.First();
                summary.AppendLine($"Latest Assessment: {latest.Summary}");
            }

            if (profile.WorkoutLogs.Any())
            {
                summary.AppendLine($"Active training: {profile.WorkoutLogs.Count} recent workouts logged");
            }

            return summary.ToString();
        }

        private string BuildNutritionSummary(UserHealthProfile profile)
        {
            var summary = new StringBuilder();
            
            if (profile.MealPlans.Any())
            {
                var latest = profile.MealPlans.First();
                summary.AppendLine($"Meal Plan: {latest.Summary}");
            }

            if (profile.SupplementsUsed.Any())
            {
                summary.AppendLine($"Supplements: {string.Join(", ", profile.SupplementsUsed.Take(3).Select(s => s.Summary))}");
            }

            return summary.ToString();
        }

        private string BuildRestrictionsSummary(UserHealthProfile profile)
        {
            var restrictions = new List<string>();
            
            // From injuries
            foreach (var injury in profile.InjuryHistory)
            {
                if (!string.IsNullOrEmpty(injury.Insights))
                {
                    restrictions.Add($"Injury: {injury.Insights}");
                }
            }

            // From allergies
            foreach (var allergy in profile.AllergiesIntolerances)
            {
                restrictions.Add($"Allergy: {allergy.Summary}");
            }

            // From medications
            foreach (var med in profile.Medications.Take(3))
            {
                if (!string.IsNullOrEmpty(med.Insights))
                {
                    restrictions.Add($"Medication: {med.Insights}");
                }
            }

            return string.Join("\n", restrictions);
        }
    }
}

