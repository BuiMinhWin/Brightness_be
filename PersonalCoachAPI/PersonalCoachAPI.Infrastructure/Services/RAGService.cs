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
                    .OrderBy(x => x.EmbeddingVector!.L2Distance(queryVector))
                    .Take(topK)
                    .ToListAsync();

                return results.Select(x => new ExerciseSearchResult
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
                    SimilarityScore = x.EmbeddingVector != null ? 
                        1.0 - (double)x.EmbeddingVector.L2Distance(queryVector) : 0.0
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
                    return new List<ExerciseSearchResult>();
                }

                var queryVector = new Vector(queryEmbedding);

                // Filter by fitness level and search
                var results = await _context.ExerciseLibrary
                    .Where(x => x.EmbeddingVector != null && 
                               (x.DifficultyLevel == fitnessLevel || fitnessLevel == "all"))
                    .OrderBy(x => x.EmbeddingVector!.L2Distance(queryVector))
                    .Take(topK)
                    .ToListAsync();

                return results.Select(x => new ExerciseSearchResult
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
                    SimilarityScore = x.EmbeddingVector != null ? 
                        1.0 - (double)x.EmbeddingVector.L2Distance(queryVector) : 0.0
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
                    .OrderBy(x => x.EmbeddingVector!.L2Distance(queryVector))
                    .Take(topK)
                    .ToListAsync();

                return results.Select(x => new HealthKnowledgeResult
                {
                    Id = x.Id,
                    Category = x.Category,
                    Topic = x.Topic,
                    Content = x.Content,
                    ContentVi = x.ContentVi,
                    Tags = JsonConvert.DeserializeObject<List<string>>(x.Tags) ?? new List<string>(),
                    SimilarityScore = x.EmbeddingVector != null ? 
                        1.0 - (double)x.EmbeddingVector.L2Distance(queryVector) : 0.0
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
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            var latestMetrics = await _context.UserHealthMetrics
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.MeasurementDate)
                .FirstOrDefaultAsync();
            var preferences = await _context.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId);

            var snapshot = new UserContextSnapshot
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HealthSummary = BuildHealthSummary(profile, latestMetrics),
                FitnessSummary = BuildFitnessSummary(profile, latestMetrics),
                GoalsSummary = profile?.FitnessGoal ?? "No goals set",
                ProgressSummary = "New user - no progress data yet",
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

        private static string BuildFitnessSummary(UserProfile? profile, UserHealthMetric? metrics)
        {
            if (profile == null) return "No fitness data available";

            return $"Level: {profile.FitnessLevel}, Goal: {profile.FitnessGoal}";
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

