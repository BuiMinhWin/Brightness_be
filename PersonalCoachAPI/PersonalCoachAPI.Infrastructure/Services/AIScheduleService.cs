using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class AIScheduleService : IAIScheduleService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly IRAGService _ragService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIScheduleService> _logger;
        private readonly HttpClient _httpClient;

        public AIScheduleService(
            PersonalCoachDbContext context,
            IRAGService ragService,
            IConfiguration configuration,
            ILogger<AIScheduleService> logger,
            HttpClient httpClient)
        {
            _context = context;
            _ragService = ragService;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<WorkoutScheduleResponse> GenerateScheduleAsync(GenerateScheduleRequest request)
        {
            try
            {
                _logger.LogInformation("Generating workout schedule for user {UserId}", request.UserId);

                // Get user context
                var userContext = await _ragService.GetUserContextAsync(request.UserId);

                // Search for relevant exercises based on goal
                var exercises = await _ragService.SearchExercisesByGoalAsync(
                    request.Goal, 
                    request.DifficultyLevel, 
                    topK: 50);

                if (!exercises.Any())
                {
                    throw new Exception("No suitable exercises found for your criteria");
                }

                // Create schedule entity
                var schedule = new WorkoutSchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    ScheduleName = $"{request.Goal} - {request.DifficultyLevel} Level",
                    Goal = request.Goal,
                    DurationDays = request.DurationDays,
                    DifficultyLevel = request.DifficultyLevel,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    AiGenerationPrompt = BuildAIPrompt(request, userContext),
                    ScheduleDays = new List<WorkoutScheduleDay>()
                };

                // Generate days
                var daysPerWeek = GetDaysPerWeek(request.DifficultyLevel);
                var minutesPerSession = GetMinutesPerSession(request.DifficultyLevel);

                for (int day = 1; day <= request.DurationDays; day++)
                {
                    var isRestDay = ShouldBeRestDay(day, daysPerWeek, request.DurationDays);
                    
                    var scheduleDay = new WorkoutScheduleDay
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = schedule.Id,
                        DayNumber = day,
                        Date = DateTime.UtcNow.AddDays(day - 1),
                        DayType = isRestDay ? "rest" : "workout",
                        FocusArea = isRestDay ? "Recovery" : GetFocusAreaForDay(day, request.Goal),
                        Completed = false,
                        CreatedAt = DateTime.UtcNow,
                        Exercises = new List<WorkoutScheduleExercise>()
                    };

                    if (!isRestDay)
                    {
                        // Select exercises for this day
                        var dayExercises = SelectExercisesForDay(exercises, scheduleDay.FocusArea, request.DifficultyLevel);
                        
                        int orderIndex = 1;
                        foreach (var exercise in dayExercises.Take(6)) // 6 exercises per day
                        {
                            var sets = GetSetsForDifficulty(request.DifficultyLevel);
                            var reps = GetRepsForExercise(exercise.Category);

                            var scheduleExercise = new WorkoutScheduleExercise
                            {
                                Id = Guid.NewGuid(),
                                ScheduleDayId = scheduleDay.Id,
                                ExerciseName = exercise.ExerciseNameVi,
                                ExerciseCategory = exercise.Category,
                                TargetMuscles = JsonConvert.SerializeObject(exercise.TargetMuscles),
                                Sets = sets,
                                Reps = reps,
                                RestSeconds = GetRestSeconds(request.DifficultyLevel),
                                OrderIndex = orderIndex++,
                                Completed = false,
                                VideoUrl = exercise.VideoUrl,
                                CreatedAt = DateTime.UtcNow
                            };

                            scheduleDay.Exercises.Add(scheduleExercise);
                        }
                    }

                    schedule.ScheduleDays.Add(scheduleDay);
                }

                // Save to database
                _context.WorkoutSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully generated schedule {ScheduleId}", schedule.Id);

                return MapToResponse(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating schedule");
                throw;
            }
        }

        public async Task<ModifiedDayResponse> ModifyScheduleDayAsync(ModifyDayRequest request)
        {
            var schedule = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(s => s.Id == request.ScheduleId);

            if (schedule == null)
            {
                throw new ArgumentException("Schedule not found");
            }

            var day = schedule.ScheduleDays.FirstOrDefault(d => d.DayNumber == request.DayNumber);
            if (day == null)
            {
                throw new ArgumentException("Day not found");
            }

            // Apply modifications based on user request
            if (!string.IsNullOrEmpty(request.NewFocusArea))
            {
                day.FocusArea = request.NewFocusArea;
            }

            if (!string.IsNullOrEmpty(request.UserRequest))
            {
                // Parse user request and adjust exercises
                await AdjustDayExercises(day, request.UserRequest, schedule.DifficultyLevel);
            }

            await _context.SaveChangesAsync();

            return new ModifiedDayResponse
            {
                ScheduleId = schedule.Id,
                DayNumber = day.DayNumber,
                ModifiedDay = MapDayToDetail(day),
                ModificationSummary = $"Đã cập nhật ngày {day.DayNumber}: {request.UserRequest}"
            };
        }

        public async Task<ScheduleAdjustmentResponse> AdjustScheduleDifficultyAsync(AdjustDifficultyRequest request)
        {
            var schedule = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(s => s.Id == request.ScheduleId);

            if (schedule == null)
            {
                throw new ArgumentException("Schedule not found");
            }

            var adjustments = new List<DayAdjustment>();

            foreach (var day in schedule.ScheduleDays.Where(d => d.DayType == "workout"))
            {
                var changes = new List<string>();

                foreach (var exercise in day.Exercises)
                {
                    if (request.AdjustmentType == "increase")
                    {
                        exercise.Sets += 1;
                        changes.Add($"{exercise.ExerciseName}: Tăng lên {exercise.Sets} sets");
                    }
                    else if (request.AdjustmentType == "decrease")
                    {
                        if (exercise.Sets > 1)
                        {
                            exercise.Sets -= 1;
                            changes.Add($"{exercise.ExerciseName}: Giảm xuống {exercise.Sets} sets");
                        }
                    }
                }

                if (changes.Any())
                {
                    adjustments.Add(new DayAdjustment
                    {
                        DayNumber = day.DayNumber,
                        Changes = changes
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new ScheduleAdjustmentResponse
            {
                ScheduleId = schedule.Id,
                AdjustmentType = request.AdjustmentType,
                Adjustments = adjustments,
                Summary = $"Đã {(request.AdjustmentType == "increase" ? "tăng" : "giảm")} độ khó cho lịch tập"
            };
        }

        public async Task<WorkoutScheduleResponse> GetScheduleAsync(Guid scheduleId)
        {
            var schedule = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException("Schedule not found");
            }

            return MapToResponse(schedule);
        }

        // Helper methods
        private async Task AdjustDayExercises(WorkoutScheduleDay day, string userRequest, string difficultyLevel)
        {
            var lowerRequest = userRequest.ToLower();

            if (lowerRequest.Contains("easier") || lowerRequest.Contains("dễ hơn"))
            {
                // Reduce sets/reps
                foreach (var exercise in day.Exercises)
                {
                    if (exercise.Sets > 2) exercise.Sets--;
                }
            }
            else if (lowerRequest.Contains("harder") || lowerRequest.Contains("khó hơn"))
            {
                // Increase sets/reps
                foreach (var exercise in day.Exercises)
                {
                    exercise.Sets++;
                }
            }
            else if (lowerRequest.Contains("more cardio") || lowerRequest.Contains("thêm cardio"))
            {
                // Replace some exercises with cardio
                var cardioExercises = await _ragService.SearchExercisesAsync("cardio running jumping", 3);
                // ... implement replacement logic
            }
        }

        private WorkoutScheduleResponse MapToResponse(WorkoutSchedule schedule)
        {
            return new WorkoutScheduleResponse
            {
                ScheduleId = schedule.Id,
                ScheduleName = schedule.ScheduleName,
                Goal = schedule.Goal,
                DifficultyLevel = schedule.DifficultyLevel,
                DurationDays = schedule.DurationDays,
                Days = schedule.ScheduleDays.OrderBy(d => d.DayNumber).Select(MapDayToDetail).ToList(),
                CreatedAt = schedule.CreatedAt
            };
        }

        private ScheduleDayDetail MapDayToDetail(WorkoutScheduleDay day)
        {
            return new ScheduleDayDetail
            {
                DayNumber = day.DayNumber,
                DayType = day.DayType,
                FocusArea = day.FocusArea,
                EstimatedDuration = 45, // Default value, could be calculated from exercises
                Exercises = day.Exercises.OrderBy(e => e.OrderIndex).Select(e => new ExerciseDetail
                {
                    ExerciseName = e.ExerciseName,
                    Category = e.ExerciseCategory,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    RestSeconds = e.RestSeconds,
                    Notes = e.Notes,
                    VideoUrl = e.VideoUrl
                }).ToList()
            };
        }

        private List<ExerciseSearchResult> SelectExercisesForDay(
            List<ExerciseSearchResult> allExercises,
            string focusArea,
            string difficultyLevel)
        {
            return focusArea.ToLower() switch
            {
                "upper body" or "thân trên" => allExercises.Where(e => 
                    e.TargetMuscles.Any(m => m.Contains("chest") || m.Contains("back") || m.Contains("shoulders"))).ToList(),
                "lower body" or "thân dưới" => allExercises.Where(e => 
                    e.TargetMuscles.Any(m => m.Contains("quads") || m.Contains("glutes") || m.Contains("legs"))).ToList(),
                "core" or "bụng" => allExercises.Where(e => 
                    e.Category == "core" || e.TargetMuscles.Any(m => m.Contains("abs") || m.Contains("core"))).ToList(),
                "cardio" => allExercises.Where(e => e.Category == "cardio").ToList(),
                "full body" or "toàn thân" => allExercises,
                _ => allExercises
            };
        }

        private static string GetFocusAreaForDay(int dayNumber, string goal)
        {
            var pattern = (dayNumber - 1) % 7;
            
            return pattern switch
            {
                0 => "Upper Body",
                1 => "Lower Body",
                2 => "Cardio",
                3 => "Core",
                4 => "Full Body",
                5 => "Cardio",
                6 => "Recovery",
                _ => "Full Body"
            };
        }

        private static bool ShouldBeRestDay(int dayNumber, int daysPerWeek, int totalDays)
        {
            if (totalDays == 7)
            {
                return dayNumber % 7 == 0; // Day 7 is rest
            }

            var dayInWeek = (dayNumber - 1) % 7 + 1;
            return dayInWeek > daysPerWeek;
        }

        private static int GetDaysPerWeek(string difficultyLevel)
        {
            return difficultyLevel.ToLower() switch
            {
                "beginner" => 3,
                "intermediate" => 4,
                "advanced" => 5,
                _ => 4
            };
        }

        private static int GetMinutesPerSession(string difficultyLevel)
        {
            return difficultyLevel.ToLower() switch
            {
                "beginner" => 30,
                "intermediate" => 45,
                "advanced" => 60,
                _ => 45
            };
        }

        private static int GetSetsForDifficulty(string difficultyLevel)
        {
            return difficultyLevel.ToLower() switch
            {
                "beginner" => 3,
                "intermediate" => 4,
                "advanced" => 5,
                _ => 3
            };
        }

        private static string GetRepsForExercise(string category)
        {
            return category.ToLower() switch
            {
                "strength" => "8-12",
                "cardio" => "30-60s",
                "core" => "15-20",
                "flexibility" => "30s hold",
                "functional" => "10-15",
                _ => "10-12"
            };
        }

        private static int GetRestSeconds(string difficultyLevel)
        {
            return difficultyLevel.ToLower() switch
            {
                "beginner" => 90,
                "intermediate" => 60,
                "advanced" => 45,
                _ => 60
            };
        }

        private static string BuildAIPrompt(GenerateScheduleRequest request, UserContextResult userContext)
        {
            var contextParts = new List<string>();
            
            if (!string.IsNullOrEmpty(userContext.HealthSummary))
                contextParts.Add($"Health: {userContext.HealthSummary}");
            
            if (!string.IsNullOrEmpty(userContext.FitnessSummary))
                contextParts.Add($"Fitness: {userContext.FitnessSummary}");
            
            if (!string.IsNullOrEmpty(userContext.GoalsSummary))
                contextParts.Add($"Goals: {userContext.GoalsSummary}");

            var userContextStr = contextParts.Any() 
                ? string.Join("\n", contextParts) 
                : "No personalization data available";

            return $@"Generate a {request.DurationDays}-day workout schedule for:
Goal: {request.Goal}
Level: {request.DifficultyLevel}
Available Days: {string.Join(", ", request.AvailableDays)}
Equipment: {string.Join(", ", request.EquipmentAvailable)}

User Personalization:
{userContextStr}

IMPORTANT: Consider the user's health data (especially InBody metrics and AI insights) when designing the workout intensity, volume, and recovery periods.";
        }
    }
}

