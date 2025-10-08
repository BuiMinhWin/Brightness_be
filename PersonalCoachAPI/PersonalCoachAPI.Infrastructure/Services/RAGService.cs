using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class RAGService : IRAGService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RAGService> _logger;
        private readonly HttpClient _httpClient;

        public RAGService(
            PersonalCoachDbContext context,
            IConfiguration configuration,
            ILogger<RAGService> logger,
            HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<List<ExerciseSearchResult>> SearchExercisesAsync(string query, int topK = 5)
        {
            try
            {
                // Generate embedding for the query
                var queryEmbedding = await GenerateEmbeddingAsync(query);
                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    _logger.LogWarning("Failed to generate embedding for query: {Query}", query);
                    return new List<ExerciseSearchResult>();
                }

                var queryVector = new Vector(queryEmbedding);

                // Semantic search using pgvector L2Distance
                var results = await _context.ExerciseLibrary
                    .Where(x => x.EmbeddingVector != null)
                    .Select(x => new
                    {
                        Exercise = x,
                        Distance = x.EmbeddingVector!.L2Distance(queryVector)
                    })
                    .OrderBy(x => x.Distance)
                    .Take(topK)
                    .ToListAsync();

                return results.Select(x => new ExerciseSearchResult
                {
                    Id = x.Exercise.Id,
                    ExerciseName = x.Exercise.ExerciseName,
                    ExerciseNameVi = x.Exercise.ExerciseNameVi,
                    Category = x.Exercise.Category,
                    DifficultyLevel = x.Exercise.DifficultyLevel,
                    TargetMuscles = JsonConvert.DeserializeObject<List<string>>(x.Exercise.TargetMuscles) ?? new List<string>(),
                    EquipmentRequired = JsonConvert.DeserializeObject<List<string>>(x.Exercise.EquipmentRequired) ?? new List<string>(),
                    Description = x.Exercise.DescriptionVi,
                    VideoUrl = x.Exercise.VideoUrl,
                    SimilarityScore = 1.0 - (double)x.Distance
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching exercises");
                return new List<ExerciseSearchResult>();
            }
        }

        public async Task<List<ExerciseSearchResult>> SearchExercisesByGoalAsync(string goal, string fitnessLevel, int topK = 10)
        {
            try
            {
                // Build search query based on goal
                var searchQuery = BuildGoalSearchQuery(goal, fitnessLevel);
                
                var queryEmbedding = await GenerateEmbeddingAsync(searchQuery);
                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    _logger.LogWarning("Failed to generate embedding, using fallback");
                }
                else
                {
                    var queryVector = new Vector(queryEmbedding);

                    // Filter by fitness level and search
                    var results = await _context.ExerciseLibrary
                        .Where(x => x.EmbeddingVector != null && 
                                   (x.DifficultyLevel == fitnessLevel || fitnessLevel == "all"))
                        .Select(x => new
                        {
                            Exercise = x,
                            Distance = x.EmbeddingVector!.L2Distance(queryVector)
                        })
                        .OrderBy(x => x.Distance)
                        .Take(topK)
                        .ToListAsync();

                    if (results.Any())
                    {
                        return results.Select(x => new ExerciseSearchResult
                        {
                            Id = x.Exercise.Id,
                            ExerciseName = x.Exercise.ExerciseName,
                            ExerciseNameVi = x.Exercise.ExerciseNameVi,
                            Category = x.Exercise.Category,
                            DifficultyLevel = x.Exercise.DifficultyLevel,
                            TargetMuscles = JsonConvert.DeserializeObject<List<string>>(x.Exercise.TargetMuscles) ?? new List<string>(),
                            EquipmentRequired = JsonConvert.DeserializeObject<List<string>>(x.Exercise.EquipmentRequired) ?? new List<string>(),
                            Description = x.Exercise.DescriptionVi,
                            VideoUrl = x.Exercise.VideoUrl,
                            SimilarityScore = 1.0 - (double)x.Distance
                        }).ToList();
                    }
                }

                // Fallback: simple filtering
                _logger.LogInformation("Using fallback exercise selection");
                var fallback = await _context.ExerciseLibrary
                    .Where(x => x.DifficultyLevel == fitnessLevel || fitnessLevel == "all")
                    .Take(topK)
                    .ToListAsync();

                return fallback.Select(x => new ExerciseSearchResult
                {
                    Id = x.Id,
                    ExerciseName = x.ExerciseName,
                    ExerciseNameVi = x.ExerciseNameVi,
                    Category = x.Category,
                    DifficultyLevel = x.DifficultyLevel,
                    TargetMuscles = JsonConvert.DeserializeObject<List<string>>(x.TargetMuscles) ?? new List<string>(),
                    EquipmentRequired = JsonConvert.DeserializeObject<List<string>>(x.EquipmentRequired) ?? new List<string>(),
                    Description = x.DescriptionVi,
                    VideoUrl = x.VideoUrl,
                    SimilarityScore = 0.5 // Default for fallback
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching exercises by goal");
                return new List<ExerciseSearchResult>();
            }
        }

        public async Task<List<HealthKnowledgeResult>> SearchHealthKnowledgeAsync(string query, int topK = 5)
        {
            try
            {
                var queryEmbedding = await GenerateEmbeddingAsync(query);
                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    return new List<HealthKnowledgeResult>();
                }

                var queryVector = new Vector(queryEmbedding);

                var results = await _context.HealthKnowledgeBase
                    .Where(x => x.EmbeddingVector != null)
                    .Select(x => new
                    {
                        Knowledge = x,
                        Distance = x.EmbeddingVector!.L2Distance(queryVector)
                    })
                    .OrderBy(x => x.Distance)
                    .Take(topK)
                    .ToListAsync();

                return results.Select(x => new HealthKnowledgeResult
                {
                    Id = x.Knowledge.Id,
                    Category = x.Knowledge.Category,
                    Topic = x.Knowledge.Topic,
                    Content = x.Knowledge.Content,
                    ContentVi = x.Knowledge.ContentVi,
                    Tags = JsonConvert.DeserializeObject<List<string>>(x.Knowledge.Tags) ?? new List<string>(),
                    SimilarityScore = 1.0 - (double)x.Distance
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching health knowledge");
                return new List<HealthKnowledgeResult>();
            }
        }

        public async Task<UserContextResult> GetUserContextAsync(Guid userId)
        {
            try
            {
                // Get latest snapshot for user
                var snapshot = await _context.UserContextSnapshots
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.SnapshotDate)
                    .FirstOrDefaultAsync();

                if (snapshot == null)
                {
                    // Create new snapshot from user data
                    snapshot = await CreateUserContextSnapshotAsync(userId);
                }

                return new UserContextResult
                {
                    UserId = userId,
                    HealthSummary = snapshot.HealthSummary,
                    FitnessSummary = snapshot.FitnessSummary,
                    GoalsSummary = snapshot.GoalsSummary,
                    ProgressSummary = snapshot.ProgressSummary,
                    SnapshotDate = snapshot.SnapshotDate,
                    SimilarityScore = 1.0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user context");
                return new UserContextResult { UserId = userId };
            }
        }

        public async Task UpdateUserContextAsync(Guid userId)
        {
            try
            {
                // Create new context snapshot
                await CreateUserContextSnapshotAsync(userId);
                _logger.LogInformation("Updated user context for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user context");
                throw;
            }
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            try
            {
                var apiKey = _configuration["GoogleAI:ApiKey"];
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={apiKey}";

                var requestBody = new
                {
                    model = "models/text-embedding-004",
                    content = new
                    {
                        parts = new[] { new { text = text } }
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Google AI API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                    return Array.Empty<float>();
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var values = result?.embedding?.values;

                if (values == null)
                {
                    return Array.Empty<float>();
                }

                var floatArray = new List<float>();
                foreach (var value in values)
                {
                    floatArray.Add((float)value);
                }

                return floatArray.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding");
                return Array.Empty<float>();
            }
        }

        private async Task<UserContextSnapshot> CreateUserContextSnapshotAsync(Guid userId)
        {
            // Get user info from AuthenticationAPI table
            var authUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            
            // Get body measurements from MeasurementAPI table
            var latestBodyMeasurement = await _context.BodyMeasurements
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.RecordedAt)
                .FirstOrDefaultAsync();
            
            // Get ALL processed health data from AIOCRService
            var allHealthData = await _context.ProcessedDataRecords
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            
            var latestInBodyData = allHealthData.FirstOrDefault(x => x.DataType == "lab_result");
            var injuries = allHealthData.Where(x => x.DataType == "injury_history").ToList();
            var allergies = allHealthData.Where(x => x.DataType == "allergy_test").ToList();
            var medications = allHealthData.Where(x => x.DataType == "prescription").ToList();
            var fitnessAssessments = allHealthData.Where(x => x.DataType == "fitness_assessment").ToList();
            var workoutLogs = allHealthData.Where(x => x.DataType == "workout_log").Take(5).ToList();
            var mealPlans = allHealthData.Where(x => x.DataType == "meal_plan").ToList();
            
            // Get our own tables (may not exist yet - graceful fallback)
            UserProfile? profile = null;
            UserHealthMetric? latestMetrics = null;
            UserPreference? preferences = null;
            
            try
            {
                profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                latestMetrics = await _context.UserHealthMetrics
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.MeasurementDate)
                    .FirstOrDefaultAsync();
                preferences = await _context.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load user profile/metrics tables - they may not exist yet");
            }

            var snapshot = new UserContextSnapshot
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HealthSummary = BuildHealthSummaryEnhanced(authUser, profile, latestMetrics, latestBodyMeasurement, latestInBodyData, injuries, allergies, medications),
                FitnessSummary = BuildFitnessSummary(profile, latestMetrics, fitnessAssessments, workoutLogs),
                GoalsSummary = profile?.FitnessGoal ?? latestBodyMeasurement?.Goal ?? "No goals set",
                ProgressSummary = BuildProgressSummary(workoutLogs, allHealthData),
                SnapshotDate = DateTime.UtcNow
            };

            // Generate embedding for the snapshot
            var snapshotText = $"{snapshot.HealthSummary} {snapshot.FitnessSummary} {snapshot.GoalsSummary}";
            var embedding = await GenerateEmbeddingAsync(snapshotText);
            if (embedding.Length > 0)
            {
                snapshot.EmbeddingVector = new Vector(embedding);
            }

            _context.UserContextSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();

            return snapshot;
        }

        private static string BuildHealthSummaryEnhanced(
            AuthUser? authUser, 
            UserProfile? profile, 
            UserHealthMetric? metrics, 
            BodyMeasurement? bodyMeasure,
            ProcessedData? inBodyData,
            List<ProcessedData> injuries,
            List<ProcessedData> allergies,
            List<ProcessedData> medications)
        {
            var parts = new List<string>();

            // Age from AuthUser
            if (authUser?.DateOfBirth != null)
            {
                var age = DateTime.UtcNow.Year - authUser.DateOfBirth.Value.Year;
                parts.Add($"Age: {age}");
            }

            // Gender from AuthUser or Profile
            var gender = authUser?.Gender ?? profile?.Gender ?? "Unknown";
            parts.Add($"Gender: {gender}");

            // Height & Weight - prioritize latest data
            var height = profile?.HeightCm ?? bodyMeasure?.HeightCm ?? 0;
            var weight = profile?.CurrentWeightKg ?? bodyMeasure?.WeightKg ?? metrics?.WeightKg ?? 0;
            
            if (height > 0) parts.Add($"Height: {height}cm");
            if (weight > 0) parts.Add($"Weight: {weight}kg");

            // Body composition
            var bodyFat = metrics?.BodyFatPercentage ?? bodyMeasure?.BodyFatPercent ?? 0;
            if (bodyFat > 0) parts.Add($"Body Fat: {bodyFat}%");

            // InBody insights from AI
            if (!string.IsNullOrEmpty(inBodyData?.Insights))
            {
                parts.Add($"InBody AI: {inBodyData.Insights.Substring(0, Math.Min(200, inBodyData.Insights.Length))}");
            }

            // Add injury history
            if (injuries.Any())
            {
                var injurySummaries = injuries.Select(i => i.Summary).Take(3);
                parts.Add($"Injuries: {string.Join("; ", injurySummaries)}");
            }

            // Add allergies
            if (allergies.Any())
            {
                var allergySummaries = allergies.Select(a => a.Summary);
                parts.Add($"Allergies: {string.Join("; ", allergySummaries)}");
            }

            // Add medications
            if (medications.Any())
            {
                parts.Add($"Medications: {medications.Count} active prescriptions");
            }

            return parts.Any() ? string.Join(", ", parts) : "No health data available";
        }

        private static string BuildHealthSummary(UserProfile? profile, UserHealthMetric? metrics)
        {
            if (profile == null) return "No health data available";

            var age = profile.DateOfBirth.HasValue ? 
                DateTime.UtcNow.Year - profile.DateOfBirth.Value.Year : 0;

            var summary = $"Age: {age}, Gender: {profile.Gender}, Height: {profile.HeightCm}cm, Weight: {profile.CurrentWeightKg}kg";
            
            if (metrics != null)
            {
                summary += $", Body Fat: {metrics.BodyFatPercentage}%";
            }

            return summary;
        }

        private static string BuildFitnessSummary(
            UserProfile? profile, 
            UserHealthMetric? metrics,
            List<ProcessedData> fitnessAssessments,
            List<ProcessedData> workoutLogs)
        {
            if (profile == null) return "No fitness data available";

            var parts = new List<string>();
            parts.Add($"Level: {profile.FitnessLevel}, Goal: {profile.FitnessGoal}");

            if (fitnessAssessments.Any())
            {
                parts.Add($"Latest Assessment: {fitnessAssessments.First().Summary}");
            }

            if (workoutLogs.Any())
            {
                parts.Add($"Recent Training: {workoutLogs.Count} workouts logged");
            }

            return string.Join(", ", parts);
        }

        private static string BuildProgressSummary(
            List<ProcessedData> workoutLogs,
            List<ProcessedData> allHealthData)
        {
            var progressPhotos = allHealthData.Where(x => x.DataType == "progress_photo").Take(2).ToList();
            
            var parts = new List<string>();
            
            if (workoutLogs.Any())
            {
                parts.Add($"{workoutLogs.Count} recent workouts completed");
            }

            if (progressPhotos.Any())
            {
                parts.Add($"Progress tracked with {progressPhotos.Count} photos");
            }

            return parts.Any() ? string.Join(", ", parts) : "New user - no progress data yet";
        }

        private static string BuildGoalSearchQuery(string goal, string fitnessLevel)
        {
            return goal.ToLower() switch
            {
                "weight_loss" or "lose_weight" => $"fat burning cardio HIIT exercises for {fitnessLevel} level",
                "muscle_gain" or "build_muscle" => $"strength training compound exercises for {fitnessLevel} bodybuilding",
                "endurance" => $"cardio endurance exercises running cycling for {fitnessLevel}",
                "flexibility" => $"stretching yoga flexibility mobility exercises for {fitnessLevel}",
                "general_fitness" => $"full body workout functional training for {fitnessLevel}",
                _ => $"{goal} exercises for {fitnessLevel} level"
            };
        }
    }
}

