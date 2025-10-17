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

        public async Task<List<ExerciseSearchResult>> SearchExercisesByGoalAsync(string goal, string fitnessLevel, List<string> availableEquipment, int topK = 10)
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

                    // Filter by fitness level, available equipment, and search
                    var results = await _context.ExerciseLibrary
                        .Where(x => x.EmbeddingVector != null && 
                                   (x.DifficultyLevel == fitnessLevel || fitnessLevel == "all"))
                        .Select(x => new
                        {
                            Exercise = x,
                            Distance = x.EmbeddingVector!.L2Distance(queryVector)
                        })
                        .OrderBy(x => x.Distance)
                        .Take(topK * 2) // Get more results to filter by equipment
                        .ToListAsync();

                    if (results.Any())
                    {
                        // Filter by equipment compatibility after loading from database
                        var embeddingResults = results
                            .Where(x => IsEquipmentCompatible(x.Exercise.EquipmentRequired, availableEquipment))
                            .Take(topK)
                            .Select(x => new ExerciseSearchResult
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

                        return embeddingResults;
                    }
                }

                // Fallback: return exercises without embedding search
                var fallbackResults = await _context.ExerciseLibrary
                    .Where(x => x.DifficultyLevel == fitnessLevel || fitnessLevel == "all")
                    .Take(topK * 2) // Get more results to filter later
                    .Select(x => new ExerciseSearchResult
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
                        SimilarityScore = 0.5 // Default similarity score for fallback
                    })
                    .ToListAsync();

                // Filter by equipment compatibility after loading from database
                var compatibleResults = fallbackResults
                    .Where(x => IsEquipmentCompatible(JsonConvert.SerializeObject(x.EquipmentRequired), availableEquipment))
                    .Take(topK)
                    .ToList();

                return compatibleResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching exercises with equipment filter");
                return new List<ExerciseSearchResult>();
            }
        }

        private bool IsEquipmentCompatible(string exerciseEquipmentJson, List<string> availableEquipment)
        {
            try
            {
                var exerciseEquipment = JsonConvert.DeserializeObject<List<string>>(exerciseEquipmentJson) ?? new List<string>();
                
                // If exercise requires no equipment, it's always compatible
                if (!exerciseEquipment.Any())
                    return true;

                // Normalize available equipment (handle plurals, synonyms, case-insensitivity)
                var normalizedAvailable = availableEquipment.Select(NormalizeEquipmentName).ToList();

                // Check if all required equipment is available (normalized comparison)
                return exerciseEquipment.All(equipment => 
                {
                    var normalizedRequired = NormalizeEquipmentName(equipment);
                    return normalizedAvailable.Any(available => 
                        string.Equals(normalizedRequired, available, StringComparison.OrdinalIgnoreCase));
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking equipment compatibility");
                return true; // Default to compatible if there's an error
            }
        }

        private string NormalizeEquipmentName(string equipment)
        {
            var normalized = equipment.ToLower().Trim();
            
            // Handle plurals - remove trailing 's'
            if (normalized.EndsWith("s") && normalized.Length > 3)
            {
                // Don't remove 's' from words like "straps", "press"
                if (!normalized.EndsWith("ss") && !normalized.EndsWith("ness"))
                {
                    normalized = normalized.TrimEnd('s');
                }
            }
            
            // Handle synonyms and variations
            var synonymMap = new Dictionary<string, string>
            {
                {"resistance band", "band"},
                {"bike", "bicycle"},
                {"stationary bike", "bicycle"},
                {"pull-up bar", "pull-up bar"},
                {"pullup bar", "pull-up bar"},
                {"yoga mat", "mat"},
                {"exercise mat", "mat"},
            };
            
            // Check if there's a synonym mapping
            foreach (var (key, value) in synonymMap)
            {
                if (normalized.Contains(key))
                {
                    return value;
                }
            }
            
            return normalized;
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

                // Get recent workout feedback for adaptive personalization
                var recentFeedback = await _context.WorkoutFeedbacks
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Build feedback summary
                var feedbackSummary = BuildFeedbackSummary(recentFeedback);

                return new UserContextResult
                {
                    UserId = userId,
                    HealthSummary = snapshot.HealthSummary,
                    FitnessSummary = snapshot.FitnessSummary,
                    GoalsSummary = snapshot.GoalsSummary,
                    ProgressSummary = snapshot.ProgressSummary + "\n" + feedbackSummary,
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

        private string BuildFeedbackSummary(List<WorkoutFeedback> feedbacks)
        {
            if (!feedbacks.Any()) return "No workout feedback yet.";

            var summary = new List<string>();
            summary.Add("Recent Workout Feedback:");

            var tooHardCount = feedbacks.Count(f => f.DifficultyRating == "too_hard" || f.DifficultyRating == "could_not_complete");
            var tooEasyCount = feedbacks.Count(f => f.DifficultyRating == "too_easy");
            var justRightCount = feedbacks.Count(f => f.DifficultyRating == "just_right");

            if (tooHardCount > 0) summary.Add($"- {tooHardCount} workouts felt too hard");
            if (tooEasyCount > 0) summary.Add($"- {tooEasyCount} workouts felt too easy");
            if (justRightCount > 0) summary.Add($"- {justRightCount} workouts felt just right");

            var avgEnergy = feedbacks.Where(f => f.EnergyLevel.HasValue).Average(f => (double?)f.EnergyLevel) ?? 0;
            if (avgEnergy > 0) summary.Add($"- Average energy level: {avgEnergy:F1}/5");

            var avgSoreness = feedbacks.Where(f => f.SorenessLevel.HasValue).Average(f => (double?)f.SorenessLevel) ?? 0;
            if (avgSoreness > 0) summary.Add($"- Average soreness: {avgSoreness:F1}/5");

            // Include specific notes from recent feedback
            var notesWithContext = feedbacks
                .Where(f => !string.IsNullOrEmpty(f.Notes))
                .Take(3)
                .Select(f => $"- Day {f.DayNumber}: {f.Notes}")
                .ToList();

            if (notesWithContext.Any())
            {
                summary.Add("Recent Notes:");
                summary.AddRange(notesWithContext);
            }

            return string.Join("\n", summary);
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

        public async Task<List<ExerciseSearchResult>> GenerateExercisesWithAIAsync(string focusArea, string difficultyLevel, List<string> availableEquipment, int count = 6)
        {
            try
            {
                _logger.LogInformation("Using AI to generate {Count} exercises for {FocusArea} ({DifficultyLevel}) with equipment: {Equipment}", 
                    count, focusArea, difficultyLevel, string.Join(", ", availableEquipment));

                var equipmentList = string.Join(", ", availableEquipment);
                var prompt = $@"Generate {Math.Min(count, 3)} specific {difficultyLevel} level exercises for {focusArea} training using ONLY this equipment: {equipmentList}.

For each exercise, provide:
1. Exercise name (in English)
2. Exercise name in Vietnamese  
3. Target muscles (as JSON array)
4. Equipment needed (from the provided list only)
5. Brief description (1-2 sentences)
6. Category (strength/cardio/flexibility)

Format as JSON array:
[
  {{
    ""exerciseName"": ""Push-ups"",
    ""exerciseNameVi"": ""Hít đất"",
    ""targetMuscles"": [""chest"", ""triceps"", ""shoulders""],
    ""equipmentRequired"": [""Bodyweight""],
    ""description"": ""A fundamental bodyweight exercise that builds upper body strength."",
    ""category"": ""strength""
  }}
]

IMPORTANT: Use ONLY equipment from this list: {equipmentList}. Keep descriptions short and concise.";

                var apiKey = _configuration["GoogleAI:ApiKey"] ?? _configuration["GOOGLE_AI_API_KEY"];
                var model = _configuration["GoogleAI:Model"] ?? "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.8,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 4096
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("AI Response: {Response}", responseContent);
                
                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);
                var generatedText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(generatedText))
                {
                    _logger.LogWarning("AI generated empty response. Full response: {Response}", responseContent);
                    return new List<ExerciseSearchResult>();
                }

                _logger.LogDebug("Generated text: {Text}", generatedText);

                // Extract JSON from markdown code blocks if present
                var jsonMatch = System.Text.RegularExpressions.Regex.Match(generatedText, @"```json\s*(.*?)\s*```", System.Text.RegularExpressions.RegexOptions.Singleline);
                var jsonText = jsonMatch.Success ? jsonMatch.Groups[1].Value : generatedText;

                // Try to find JSON array in the text
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    var arrayMatch = System.Text.RegularExpressions.Regex.Match(generatedText, @"\[.*\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                    if (arrayMatch.Success)
                    {
                        jsonText = arrayMatch.Value;
                    }
                }

                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    _logger.LogWarning("Could not extract JSON from AI response: {Text}", generatedText);
                    return new List<ExerciseSearchResult>();
                }

                _logger.LogDebug("Extracted JSON: {Json}", jsonText);

                List<GeneratedExercise> generatedExercises;
                try
                {
                    generatedExercises = JsonConvert.DeserializeObject<List<GeneratedExercise>>(jsonText);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse AI-generated JSON: {Json}", jsonText);
                    _logger.LogWarning("Using fallback exercises due to JSON parsing error");
                    return CreateFallbackExercises(focusArea, difficultyLevel, availableEquipment, count);
                }

                if (generatedExercises == null || !generatedExercises.Any())
                {
                    _logger.LogWarning("Failed to parse AI-generated exercises, using fallback");
                    return CreateFallbackExercises(focusArea, difficultyLevel, availableEquipment, count);
                }

                // Convert to ExerciseSearchResult and save to database for future use
                var results = new List<ExerciseSearchResult>();
                foreach (var genEx in generatedExercises)
                {
                    var exerciseId = Guid.NewGuid();
                    
                    // Create exercise library entry
                    var exerciseLibEntry = new ExerciseLibrary
                    {
                        Id = exerciseId,
                        ExerciseName = genEx.ExerciseName,
                        ExerciseNameVi = genEx.ExerciseNameVi,
                        Category = genEx.Category,
                        DifficultyLevel = difficultyLevel,
                        EquipmentRequired = JsonConvert.SerializeObject(genEx.EquipmentRequired),
                        TargetMuscles = JsonConvert.SerializeObject(genEx.TargetMuscles),
                        SecondaryMuscles = "[]",
                        Description = genEx.Description,
                        DescriptionVi = genEx.Description, // Use same for now
                        Instructions = "[]",
                        Benefits = "[]",
                        CommonMistakes = "[]",
                        Variations = "[]",
                        VideoUrl = "",
                        ImageUrl = "",
                        CaloriesPerMinute = genEx.Category == "cardio" ? 10 : 7,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Generate embedding for the exercise
                    var embeddingText = $"{genEx.ExerciseName} {genEx.Description} {string.Join(" ", genEx.TargetMuscles)} {difficultyLevel}";
                    var embedding = await GenerateEmbeddingAsync(embeddingText);
                    if (embedding != null && embedding.Length > 0)
                    {
                        exerciseLibEntry.EmbeddingVector = new Vector(embedding);
                    }

                    // Add to database
                    _context.ExerciseLibrary.Add(exerciseLibEntry);

                    // Add to results
                    results.Add(new ExerciseSearchResult
                    {
                        Id = exerciseId,
                        ExerciseName = genEx.ExerciseName,
                        ExerciseNameVi = genEx.ExerciseNameVi,
                        Category = genEx.Category,
                        DifficultyLevel = difficultyLevel,
                        TargetMuscles = genEx.TargetMuscles,
                        EquipmentRequired = genEx.EquipmentRequired,
                        Description = genEx.Description,
                        VideoUrl = "",
                        SimilarityScore = 0.95 // High score for AI-generated
                    });
                }

                // Save all new exercises to database
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully generated and saved {Count} AI exercises to RAG", results.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating exercises with AI");
                
                // Fallback: Create basic exercises based on focus area
                return CreateFallbackExercises(focusArea, difficultyLevel, availableEquipment, count);
            }
        }

        private List<ExerciseSearchResult> CreateFallbackExercises(string focusArea, string difficultyLevel, List<string> availableEquipment, int count)
        {
            _logger.LogInformation("Creating fallback exercises for {FocusArea} ({DifficultyLevel})", focusArea, difficultyLevel);
            
            var exercises = new List<ExerciseSearchResult>();
            var equipment = availableEquipment.FirstOrDefault() ?? "Bodyweight";
            
            // Basic exercise templates based on focus area
            var exerciseTemplates = focusArea.ToLower() switch
            {
                "upper body" or "upper_body" => new[]
                {
                    ("Push-ups", "Hít đất", new[] { "chest", "triceps", "shoulders" }),
                    ("Diamond Push-ups", "Hít đất kim cương", new[] { "chest", "triceps" }),
                    ("Pike Push-ups", "Hít đất chữ V", new[] { "shoulders", "triceps" }),
                    ("Arm Circles", "Xoay tay", new[] { "shoulders" }),
                    ("Tricep Dips", "Chống đẩy cơ tam đầu", new[] { "triceps", "shoulders" }),
                    ("Plank", "Tấm ván", new[] { "core", "shoulders" })
                },
                "core" => new[]
                {
                    ("Plank", "Tấm ván", new[] { "core", "abs" }),
                    ("Mountain Climbers", "Leo núi", new[] { "core", "cardio" }),
                    ("Russian Twists", "Xoay người Nga", new[] { "obliques", "core" }),
                    ("Bicycle Crunches", "Gập bụng đạp xe", new[] { "abs", "obliques" }),
                    ("Dead Bug", "Con bọ chết", new[] { "core", "abs" }),
                    ("Leg Raises", "Nâng chân", new[] { "lower abs" })
                },
                "lower body" or "lower_body" => new[]
                {
                    ("Squats", "Squat", new[] { "quads", "glutes" }),
                    ("Lunges", "Lunge", new[] { "quads", "glutes", "hamstrings" }),
                    ("Jump Squats", "Squat nhảy", new[] { "quads", "glutes", "cardio" }),
                    ("Calf Raises", "Nâng bắp chân", new[] { "calves" }),
                    ("Glute Bridges", "Cầu mông", new[] { "glutes", "hamstrings" }),
                    ("Wall Sit", "Ngồi tường", new[] { "quads", "glutes" })
                },
                _ => new[]
                {
                    ("Jumping Jacks", "Nhảy dang tay", new[] { "cardio", "full body" }),
                    ("Burpees", "Burpee", new[] { "cardio", "full body" }),
                    ("High Knees", "Nâng cao gối", new[] { "cardio", "legs" }),
                    ("Plank", "Tấm ván", new[] { "core", "shoulders" }),
                    ("Push-ups", "Hít đất", new[] { "chest", "triceps" }),
                    ("Squats", "Squat", new[] { "quads", "glutes" })
                }
            };
            
            for (int i = 0; i < Math.Min(count, exerciseTemplates.Length); i++)
            {
                var template = exerciseTemplates[i];
                var exerciseId = Guid.NewGuid();
                
                // Create exercise library entry
                var exerciseLibEntry = new ExerciseLibrary
                {
                    Id = exerciseId,
                    ExerciseName = template.Item1,
                    ExerciseNameVi = template.Item2,
                    Category = template.Item3.Contains("cardio") ? "cardio" : "strength",
                    DifficultyLevel = difficultyLevel,
                    EquipmentRequired = JsonConvert.SerializeObject(new[] { equipment }),
                    TargetMuscles = JsonConvert.SerializeObject(template.Item3),
                    SecondaryMuscles = "[]",
                    Description = $"Basic {difficultyLevel} level {template.Item1} exercise",
                    DescriptionVi = $"Bài tập {template.Item2} cơ bản cho trình độ {difficultyLevel}",
                    Instructions = "[]",
                    Benefits = "[]",
                    CommonMistakes = "[]",
                    Variations = "[]",
                    VideoUrl = "",
                    ImageUrl = "",
                    CaloriesPerMinute = template.Item3.Contains("cardio") ? 10 : 7,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Add to database
                _context.ExerciseLibrary.Add(exerciseLibEntry);
                
                // Add to results
                exercises.Add(new ExerciseSearchResult
                {
                    Id = exerciseId,
                    ExerciseName = template.Item1,
                    ExerciseNameVi = template.Item2,
                    Category = exerciseLibEntry.Category,
                    DifficultyLevel = difficultyLevel,
                    TargetMuscles = template.Item3.ToList(),
                    EquipmentRequired = new[] { equipment }.ToList(),
                    Description = exerciseLibEntry.Description,
                    VideoUrl = "",
                    SimilarityScore = 0.8 // Good score for fallback
                });
            }
            
            // Save to database
            try
            {
                _context.SaveChangesAsync().Wait();
                _logger.LogInformation("Successfully created {Count} fallback exercises", exercises.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save fallback exercises to database");
            }
            
            return exercises;
        }

        // Helper classes for AI generation
        private class GeneratedExercise
        {
            public string ExerciseName { get; set; } = "";
            public string ExerciseNameVi { get; set; } = "";
            public List<string> TargetMuscles { get; set; } = new();
            public List<string> EquipmentRequired { get; set; } = new();
            public string Description { get; set; } = "";
            public string Category { get; set; } = "";
        }

        private class GeminiResponse
        {
            public GeminiCandidate[]? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            public GeminiPart[]? Parts { get; set; }
        }

        private class GeminiPart
        {
            public string? Text { get; set; }
        }
    }
}

