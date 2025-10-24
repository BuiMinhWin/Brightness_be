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
        private readonly IAIUsageLimitService _aiUsageLimitService;

        public AIScheduleService(
            PersonalCoachDbContext context,
            IRAGService ragService,
            IConfiguration configuration,
            ILogger<AIScheduleService> logger,
            HttpClient httpClient,
            IAIUsageLimitService aiUsageLimitService)
        {
            _context = context;
            _ragService = ragService;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _aiUsageLimitService = aiUsageLimitService;
        }

        public async Task<WorkoutScheduleResponse> GenerateScheduleAsync(GenerateScheduleRequest request)
        {
            // Check AI usage limits before generating
            var usageLimits = await _aiUsageLimitService.GetUsageLimitsAsync(request.UserId);
            if (!usageLimits.CanGenerate)
            {
                throw new InvalidOperationException(
                    $"Monthly AI generation limit reached ({usageLimits.MonthlyLimit}). " +
                    $"Resets in {usageLimits.DaysUntilReset} days. " +
                    $"Used: {usageLimits.UsedThisMonth}/{usageLimits.MonthlyLimit}");
            }

            // Use transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Generating workout schedule for user {UserId} (AI {Used}/{Limit})", 
                    request.UserId, usageLimits.UsedThisMonth, usageLimits.MonthlyLimit);

                // Archive all existing active schedules for this user
                var existingSchedules = await _context.WorkoutSchedules
                    .Where(s => s.UserId == request.UserId && s.IsLatest)
                    .ToListAsync();

                if (existingSchedules.Any())
                {
                    _logger.LogInformation("Archiving {Count} existing schedules for user {UserId}", 
                        existingSchedules.Count, request.UserId);
                    
                    foreach (var existingSchedule in existingSchedules)
                    {
                        existingSchedule.IsLatest = false;
                        existingSchedule.Status = "archived";
                        existingSchedule.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Get user context
                var userContext = await _ragService.GetUserContextAsync(request.UserId);

                // Search for relevant exercises based on goal and available equipment
                var exercises = await _ragService.SearchExercisesByGoalAsync(
                    request.Goal, 
                    request.DifficultyLevel, 
                    request.EquipmentAvailable,
                    topK: 50);

                // If no exercises found with equipment filter, try without equipment filter
                if (!exercises.Any())
                {
                    _logger.LogWarning("No exercises found with equipment filter. Trying without equipment filter.");
                    exercises = await _ragService.SearchExercisesByGoalAsync(
                        request.Goal, 
                        request.DifficultyLevel, 
                        topK: 50);
                }

                if (!exercises.Any())
                {
                    _logger.LogError("No exercises found even without equipment filter. Goal: {Goal}, Difficulty: {Difficulty}", request.Goal, request.DifficultyLevel);
                    throw new Exception("No suitable exercises found for your criteria. Please try different settings.");
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

                // Generate days based on available days selection
                var minutesPerSession = GetMinutesPerSession(request.DifficultyLevel);
                
                // Convert available days to day numbers (0=Sunday, 1=Monday, etc.)
                var availableDayNumbers = request.AvailableDays.Select(day => 
                    day.ToLower() switch
                    {
                        "sunday" => 0,
                        "monday" => 1,
                        "tuesday" => 2,
                        "wednesday" => 3,
                        "thursday" => 4,
                        "friday" => 5,
                        "saturday" => 6,
                        _ => -1
                    }).Where(dayNum => dayNum >= 0).OrderBy(d => d).ToList();

                int workoutDayCount = 0;
                int dayNumber = 1;
                
                // Determine start date based on user preference
                var startDate = request.StartFromToday 
                    ? DateTime.UtcNow.Date 
                    : DateTime.UtcNow.Date.AddDays(1);
                var currentCheckDate = startDate;
                
                // Generate schedule for the duration
                // DurationDays represents the number of WORKOUT SESSIONS to generate
                // We'll iterate through calendar days and create workouts on available days
                // until we reach the target number of workout sessions
                int maxCalendarDays = request.DurationDays * 7; // Safety limit to prevent infinite loop
                int calendarDaysChecked = 0;
                
                while (workoutDayCount < request.DurationDays && calendarDaysChecked < maxCalendarDays)
                {
                    var dayOfWeek = (int)currentCheckDate.DayOfWeek;
                    
                    // Increment calendar days checked to prevent infinite loops
                    calendarDaysChecked++;
                    
                    // Skip days not in available days - don't create schedule entries for them
                    if (!availableDayNumbers.Contains(dayOfWeek))
                    {
                        currentCheckDate = currentCheckDate.AddDays(1);
                        continue;
                    }
                    
                    // Create a workout day (no automatic rest days - users control this via available days)
                    var scheduleDay = new WorkoutScheduleDay
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = schedule.Id,
                        DayNumber = dayNumber++,
                        Date = currentCheckDate,
                        DayType = "workout",
                        FocusArea = GetFocusAreaForDay(workoutDayCount + 1, request.Goal),
                        Completed = false,
                        CreatedAt = DateTime.UtcNow,
                        Exercises = new List<WorkoutScheduleExercise>()
                    };

                    // Select exercises for this day
                    var dayExercises = SelectExercisesForDay(exercises, scheduleDay.FocusArea, request.DifficultyLevel);
                    
                    // If no exercises found after equipment filtering, search for bodyweight alternatives
                    if (!dayExercises.Any())
                    {
                        _logger.LogWarning("No exercises found for {FocusArea} with equipment filter. Searching for bodyweight alternatives.", scheduleDay.FocusArea);
                        var bodyweightExercises = await _ragService.SearchExercisesByGoalAsync(
                            scheduleDay.FocusArea, 
                            request.DifficultyLevel, 
                            new List<string> { "Bodyweight", "None" },
                            topK: 20);
                        dayExercises = SelectExercisesForDay(bodyweightExercises, scheduleDay.FocusArea, request.DifficultyLevel);
                    }
                    
                    // If still no exercises found, use AI to generate them
                    if (!dayExercises.Any())
                    {
                        _logger.LogWarning("No exercises found even with bodyweight filter. Using AI to generate exercises for {FocusArea}.", scheduleDay.FocusArea);
                        dayExercises = await _ragService.GenerateExercisesWithAIAsync(
                            scheduleDay.FocusArea, 
                            request.DifficultyLevel, 
                            request.EquipmentAvailable.Any() ? request.EquipmentAvailable : new List<string> { "Bodyweight", "None" },
                            count: 6);
                    }
                    
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
                    
                    scheduleDay.EstimatedDuration = minutesPerSession;
                    scheduleDay.Status = "scheduled";
                    workoutDayCount++;

                    schedule.ScheduleDays.Add(scheduleDay);
                    
                    // Move to next day
                    currentCheckDate = currentCheckDate.AddDays(1);
                }

                // Save to database
                _context.WorkoutSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                
                // Track AI usage
                await _aiUsageLimitService.TrackGenerationAsync(request.UserId, "schedule_create", schedule.Id);
                
                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully generated schedule {ScheduleId} and archived {Count} old schedules", 
                    schedule.Id, existingSchedules.Count);

                return MapToResponse(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating schedule");
                await transaction.RollbackAsync();
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

        public async Task<List<WorkoutScheduleResponse>> GetUserSchedulesAsync(Guid userId)
        {
            var schedules = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                .ThenInclude(d => d.Exercises)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.IsLatest)  // Prioritize IsLatest flag
                .ThenByDescending(s => s.CreatedAt)  // Then by creation date
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        public async Task MarkExerciseCompleteAsync(Guid exerciseId, bool completed, int? actualSets, int? actualReps)
        {
            var exercise = await _context.WorkoutScheduleExercises
                .Include(e => e.ScheduleDay)
                    .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(e => e.Id == exerciseId);

            if (exercise == null)
            {
                throw new ArgumentException("Exercise not found");
            }

            exercise.Completed = completed;
            exercise.ActualSets = actualSets;
            exercise.ActualReps = actualReps;

            // Check if all exercises in the day are completed
            var day = exercise.ScheduleDay;
            if (day != null)
            {
                var allExercisesCompleted = day.Exercises.All(e => e.Completed || e.Id == exerciseId && completed);
                
                if (allExercisesCompleted && !day.Completed)
                {
                    // Mark the entire day as completed
                    day.Completed = true;
                    day.Status = "completed";
                    day.CompletionDate = DateTime.UtcNow;
                    day.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Marked day {DayId} as completed", day.Id);
                }
                else if (!allExercisesCompleted && day.Completed)
                {
                    // If an exercise was unmarked, unmark the day
                    day.Completed = false;
                    day.Status = "scheduled";
                    day.CompletionDate = null;
                    day.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Unmarked day {DayId} as incomplete", day.Id);
                }
            }

            await _context.SaveChangesAsync();
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
                ScheduleDayId = day.Id,
                DayNumber = day.DayNumber,
                Date = day.Date,
                DayType = day.DayType,
                FocusArea = day.FocusArea,
                EstimatedDuration = day.EstimatedDuration,
                Exercises = day.Exercises.OrderBy(e => e.OrderIndex).Select(e => new ExerciseDetail
                {
                    ExerciseId = e.Id,
                    ExerciseName = e.ExerciseName,
                    Category = e.ExerciseCategory,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    RestSeconds = e.RestSeconds,
                    Notes = e.Notes,
                    VideoUrl = e.VideoUrl,
                    Completed = e.Completed,
                    ActualSets = e.ActualSets,
                    ActualReps = e.ActualReps
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
            // Cycle through 6 main focus areas (no Recovery day)
            var pattern = (dayNumber - 1) % 6;

            return pattern switch
            {
                0 => "Upper Body",
                1 => "Lower Body",
                2 => "Cardio",
                3 => "Core",
                4 => "Full Body",
                5 => "Cardio",
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

        // ============================================
        // NEW FULL SUITE FEATURES
        // ============================================

        public async Task SubmitWorkoutFeedbackAsync(SubmitWorkoutFeedbackRequest request)
        {
            var feedback = new WorkoutFeedback
            {
                Id = Guid.NewGuid(),
                UserId = (await _context.WorkoutSchedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId))?.UserId ?? Guid.Empty,
                ScheduleId = request.ScheduleId,
                ScheduleDayId = request.ScheduleDayId,
                DayNumber = request.DayNumber,
                FeedbackType = "difficulty",
                DifficultyRating = request.DifficultyRating,
                EnergyLevel = request.EnergyLevel,
                SorenessLevel = request.SorenessLevel,
                TookLongerThanExpected = request.TookLongerThanExpected,
                ActualDurationMinutes = request.ActualDurationMinutes,
                Notes = request.Notes,
                ContextTags = request.ContextTags != null ? JsonConvert.SerializeObject(request.ContextTags) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkoutFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Workout feedback submitted for Schedule {ScheduleId}, Day {DayNumber}: {Rating}", 
                request.ScheduleId, request.DayNumber, request.DifficultyRating);
        }

        public async Task<WorkoutScheduleResponse> RegenerateScheduleFromDayAsync(RegenerateScheduleRequest request)
        {
            // Get current schedule first to get userId
            var currentSchedule = await _context.WorkoutSchedules
                .FirstOrDefaultAsync(s => s.Id == request.CurrentScheduleId);

            if (currentSchedule == null)
            {
                throw new Exception("Schedule not found");
            }

            // Check AI usage limits before regenerating
            var usageLimits = await _aiUsageLimitService.GetUsageLimitsAsync(currentSchedule.UserId);
            if (!usageLimits.CanGenerate)
            {
                throw new InvalidOperationException(
                    $"Monthly AI generation limit reached ({usageLimits.MonthlyLimit}). " +
                    $"Resets in {usageLimits.DaysUntilReset} days. " +
                    $"Used: {usageLimits.UsedThisMonth}/{usageLimits.MonthlyLimit}");
            }

            try
            {
                _logger.LogInformation("Regenerating schedule from day {FromDay} for schedule {ScheduleId} (AI {Used}/{Limit})", 
                    request.FromDayNumber, request.CurrentScheduleId, usageLimits.UsedThisMonth, usageLimits.MonthlyLimit);

                // Reload schedule with includes
                currentSchedule = await _context.WorkoutSchedules
                    .Include(s => s.ScheduleDays)
                        .ThenInclude(d => d.Exercises)
                    .FirstOrDefaultAsync(s => s.Id == request.CurrentScheduleId);

                if (currentSchedule == null)
                {
                    throw new Exception("Schedule not found");
                }

                // Archive current schedule
                currentSchedule.Status = "archived";
                currentSchedule.IsLatest = false;
                currentSchedule.UpdatedAt = DateTime.UtcNow;

                // Get completed days to preserve
                var completedDays = currentSchedule.ScheduleDays
                    .Where(d => d.DayNumber < request.FromDayNumber && d.Completed)
                    .OrderBy(d => d.DayNumber)
                    .ToList();

                // Get user context for personalization
                var userContext = await _ragService.GetUserContextAsync(currentSchedule.UserId);

                // Get user feedback history for adaptive difficulty
                var feedbackHistory = await _context.WorkoutFeedbacks
                    .Where(f => f.UserId == currentSchedule.UserId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Determine new difficulty based on feedback
                var newDifficulty = request.NewDifficultyLevel ?? 
                    DetermineDifficultyFromFeedback(currentSchedule.DifficultyLevel, feedbackHistory);

                // Get available days (use new if provided, otherwise from user preferences)
                var userPreferences = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == currentSchedule.UserId);
                var availableDays = request.NewAvailableDays ?? 
                    JsonConvert.DeserializeObject<List<string>>(
                        userPreferences?.AvailableDays ?? "[]") ?? new List<string>();

                if (!availableDays.Any())
                {
                    // Default to weekdays if no preference
                    availableDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
                }

                // Calculate remaining workouts needed
                var remainingWorkouts = currentSchedule.DurationDays - completedDays.Count;

                // Search for exercises with adaptive filtering based on feedback
                var equipmentAvailable = JsonConvert.DeserializeObject<List<string>>(
                    userPreferences?.EquipmentAvailable ?? "[]") ?? new List<string>();

                var exercises = await _ragService.SearchExercisesByGoalAsync(
                    currentSchedule.Goal,
                    newDifficulty,
                    equipmentAvailable,
                    topK: 50);

                // Fallback if no exercises found
                if (!exercises.Any())
                {
                    _logger.LogWarning("No exercises found with equipment filter. Trying without equipment filter.");
                    exercises = await _ragService.SearchExercisesByGoalAsync(
                        currentSchedule.Goal,
                        newDifficulty,
                        topK: 50);
                }

                if (!exercises.Any())
                {
                    throw new Exception("No suitable exercises found for regeneration");
                }

                // Create new schedule version
                var newSchedule = new WorkoutSchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = currentSchedule.UserId,
                    ScheduleName = $"{currentSchedule.Goal} - {newDifficulty} Level (v{currentSchedule.Version + 1})",
                    Goal = currentSchedule.Goal,
                    DurationDays = currentSchedule.DurationDays,
                    DifficultyLevel = newDifficulty,
                    Status = "active",
                    CreatedByAi = true,
                    Version = currentSchedule.Version + 1,
                    ParentScheduleId = currentSchedule.Id,
                    RegeneratedFromDay = request.FromDayNumber,
                    RegenerationReason = request.Reason,
                    IsLatest = true,
                    CreatedAt = DateTime.UtcNow,
                    AiGenerationPrompt = BuildRegenerationPrompt(request, feedbackHistory, userContext),
                    ScheduleDays = new List<WorkoutScheduleDay>()
                };

                // Copy completed days if requested
                if (request.PreserveCompleted && completedDays.Any())
                {
                    foreach (var completedDay in completedDays)
                    {
                        var copiedDay = new WorkoutScheduleDay
                        {
                            Id = Guid.NewGuid(),
                            ScheduleId = newSchedule.Id,
                            DayNumber = completedDay.DayNumber,
                            Date = completedDay.Date,
                            DayType = completedDay.DayType,
                            FocusArea = completedDay.FocusArea,
                            EstimatedDuration = completedDay.EstimatedDuration,
                            Status = completedDay.Status,
                            Completed = completedDay.Completed,
                            UserNotes = completedDay.UserNotes,
                            CreatedAt = completedDay.CreatedAt,
                            Exercises = completedDay.Exercises.Select(e => new WorkoutScheduleExercise
                            {
                                Id = Guid.NewGuid(),
                                ScheduleDayId = Guid.Empty, // Will be set after day is added
                                ExerciseName = e.ExerciseName,
                                ExerciseCategory = e.ExerciseCategory,
                                TargetMuscles = e.TargetMuscles,
                                Sets = e.Sets,
                                Reps = e.Reps,
                                RestSeconds = e.RestSeconds,
                                OrderIndex = e.OrderIndex,
                                Completed = e.Completed,
                                ActualSets = e.ActualSets,
                                ActualReps = e.ActualReps,
                                Notes = e.Notes,
                                VideoUrl = e.VideoUrl,
                                CreatedAt = e.CreatedAt
                            }).ToList()
                        };

                        // Update exercise ScheduleDayId
                        foreach (var exercise in copiedDay.Exercises)
                        {
                            exercise.ScheduleDayId = copiedDay.Id;
                        }

                        newSchedule.ScheduleDays.Add(copiedDay);
                    }
                }

                // Generate new days from the specified day forward
                var minutesPerSession = GetMinutesPerSession(newDifficulty);
                var availableDayNumbers = availableDays.Select(day => day switch
                {
                    "Sunday" => 0,
                    "Monday" => 1,
                    "Tuesday" => 2,
                    "Wednesday" => 3,
                    "Thursday" => 4,
                    "Friday" => 5,
                    "Saturday" => 6,
                    _ => -1
                }).Where(d => d >= 0).ToList();

                int workoutDayCount = completedDays.Count;
                // Start from the next day number after completed days
                int dayNumber = completedDays.Any() ? completedDays.Max(d => d.DayNumber) + 1 : request.FromDayNumber;
                var lastCompletedDate = completedDays.Any() 
                    ? completedDays.Max(d => d.Date) 
                    : DateTime.UtcNow.Date;
                var currentCheckDate = lastCompletedDate.AddDays(1);

                int maxCalendarDays = remainingWorkouts * 7;
                int calendarDaysChecked = 0;

                while (workoutDayCount < currentSchedule.DurationDays && calendarDaysChecked < maxCalendarDays)
                {
                    var dayOfWeek = (int)currentCheckDate.DayOfWeek;
                    calendarDaysChecked++;

                    if (!availableDayNumbers.Contains(dayOfWeek))
                    {
                        currentCheckDate = currentCheckDate.AddDays(1);
                        continue;
                    }

                    var scheduleDay = new WorkoutScheduleDay
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = newSchedule.Id,
                        DayNumber = dayNumber++,
                        Date = currentCheckDate,
                        DayType = "workout",
                        FocusArea = GetFocusAreaForDay(workoutDayCount + 1, currentSchedule.Goal),
                        Completed = false,
                        CreatedAt = DateTime.UtcNow,
                        Exercises = new List<WorkoutScheduleExercise>()
                    };

                    // Select exercises for this day (use standard selection for now)
                    var dayExercises = SelectExercisesForDay(exercises, scheduleDay.FocusArea, newDifficulty);

                    if (!dayExercises.Any())
                    {
                        _logger.LogWarning("No exercises found for {FocusArea}. Searching for bodyweight alternatives.", scheduleDay.FocusArea);
                        var bodyweightExercises = await _ragService.SearchExercisesByGoalAsync(
                            scheduleDay.FocusArea,
                            newDifficulty,
                            new List<string> { "Bodyweight", "None" },
                            topK: 20);
                        dayExercises = SelectExercisesForDay(bodyweightExercises, scheduleDay.FocusArea, newDifficulty);
                    }

                    int orderIndex = 1;
                    foreach (var exercise in dayExercises.Take(6))
                    {
                        var sets = GetSetsForDifficultyAdaptive(newDifficulty, feedbackHistory);
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
                            RestSeconds = GetRestSeconds(newDifficulty),
                            OrderIndex = orderIndex++,
                            Completed = false,
                            VideoUrl = exercise.VideoUrl,
                            CreatedAt = DateTime.UtcNow
                        };

                        scheduleDay.Exercises.Add(scheduleExercise);
                    }

                    scheduleDay.EstimatedDuration = minutesPerSession;
                    scheduleDay.Status = "scheduled";
                    workoutDayCount++;

                    newSchedule.ScheduleDays.Add(scheduleDay);
                    currentCheckDate = currentCheckDate.AddDays(1);
                }

                // Save new schedule
                _context.WorkoutSchedules.Add(newSchedule);
                
                // Track AI usage for regeneration
                await _aiUsageLimitService.TrackGenerationAsync(currentSchedule.UserId, "schedule_regenerate", newSchedule.Id);

                // Log edit history
                var editHistory = new ScheduleEditHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = currentSchedule.UserId,
                    ScheduleId = currentSchedule.Id,
                    EditType = "regenerated",
                    EditDescription = $"Regenerated from day {request.FromDayNumber}: {request.Reason}",
                    BeforeValue = JsonConvert.SerializeObject(new { Version = currentSchedule.Version, Status = "active" }),
                    AfterValue = JsonConvert.SerializeObject(new { Version = newSchedule.Version, NewScheduleId = newSchedule.Id }),
                    EditSource = "user",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ScheduleEditHistories.Add(editHistory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully regenerated schedule. New version: {Version}", newSchedule.Version);

                return MapToResponse(newSchedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating schedule");
                throw;
            }
        }

        public async Task<ScheduleDayDetail> EditWorkoutDayAsync(EditWorkoutDayRequest request)
        {
            var scheduleDay = await _context.WorkoutScheduleDays
                .Include(d => d.Exercises)
                .FirstOrDefaultAsync(d => d.Id == request.ScheduleDayId);

            if (scheduleDay == null)
            {
                throw new Exception("Schedule day not found");
            }

            var beforeValue = JsonConvert.SerializeObject(MapDayToDetail(scheduleDay));

            // Update focus area if provided
            if (!string.IsNullOrEmpty(request.NewFocusArea))
            {
                scheduleDay.FocusArea = request.NewFocusArea;
            }

            // Modify existing exercises
            if (request.ModifiedExercises != null)
            {
                foreach (var modReq in request.ModifiedExercises)
                {
                    var exercise = scheduleDay.Exercises.FirstOrDefault(e => e.Id == modReq.ExerciseId);
                    if (exercise != null)
                    {
                        if (modReq.NewSets.HasValue) exercise.Sets = modReq.NewSets.Value;
                        if (!string.IsNullOrEmpty(modReq.NewReps)) exercise.Reps = modReq.NewReps;
                        if (modReq.NewRestSeconds.HasValue) exercise.RestSeconds = modReq.NewRestSeconds.Value;
                        if (!string.IsNullOrEmpty(modReq.NewNotes)) exercise.Notes = modReq.NewNotes;
                    }
                }
            }

            // Remove exercises
            if (request.ExercisesToRemove != null)
            {
                var exercisesToDelete = scheduleDay.Exercises
                    .Where(e => request.ExercisesToRemove.Contains(e.Id))
                    .ToList();

                foreach (var exercise in exercisesToDelete)
                {
                    _context.WorkoutScheduleExercises.Remove(exercise);
                }
            }

            // Add new exercises
            if (request.ExercisesToAdd != null)
            {
                foreach (var addReq in request.ExercisesToAdd)
                {
                    var newExercise = new WorkoutScheduleExercise
                    {
                        Id = Guid.NewGuid(),
                        ScheduleDayId = scheduleDay.Id,
                        ExerciseName = addReq.ExerciseName,
                        Sets = addReq.Sets,
                        Reps = addReq.Reps,
                        RestSeconds = addReq.RestSeconds,
                        OrderIndex = addReq.OrderIndex,
                        CreatedAt = DateTime.UtcNow
                    };

                    scheduleDay.Exercises.Add(newExercise);
                }
            }

            // Log edit history
            var afterValue = JsonConvert.SerializeObject(MapDayToDetail(scheduleDay));
            var editHistory = new ScheduleEditHistory
            {
                Id = Guid.NewGuid(),
                UserId = (await _context.WorkoutSchedules.FirstOrDefaultAsync(s => s.Id == scheduleDay.ScheduleId))?.UserId ?? Guid.Empty,
                ScheduleId = scheduleDay.ScheduleId,
                ScheduleDayId = scheduleDay.Id,
                EditType = "day_modified",
                EditDescription = $"Modified day {scheduleDay.DayNumber}",
                BeforeValue = beforeValue,
                AfterValue = afterValue,
                EditSource = "user",
                CreatedAt = DateTime.UtcNow
            };

            _context.ScheduleEditHistories.Add(editHistory);
            await _context.SaveChangesAsync();

            return MapDayToDetail(scheduleDay);
        }

        public async Task<ScheduleHistoryResponse> GetScheduleHistoryAsync(Guid userId)
        {
            var schedules = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                    .ThenInclude(d => d.Exercises)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var versions = schedules.Select(s => new ScheduleVersionInfo
            {
                ScheduleId = s.Id,
                Version = s.Version,
                CreatedAt = s.CreatedAt,
                IsLatest = s.IsLatest,
                Status = s.Status,
                TotalDays = s.ScheduleDays.Count,
                CompletedDays = s.ScheduleDays.Count(d => d.Completed),
                CompletionPercentage = s.ScheduleDays.Any() 
                    ? (int)((double)s.ScheduleDays.Count(d => d.Completed) / s.ScheduleDays.Count * 100) 
                    : 0,
                RegenerationReason = s.RegenerationReason
            }).ToList();

            var analytics = await GetScheduleAnalyticsAsync(userId);

            return new ScheduleHistoryResponse
            {
                Versions = versions,
                Analytics = analytics
            };
        }

        public async Task<ScheduleAnalytics> GetScheduleAnalyticsAsync(Guid userId)
        {
            // Get only the latest active schedule for analytics
            var latestSchedule = await _context.WorkoutSchedules
                .Include(s => s.ScheduleDays)
                    .ThenInclude(d => d.Exercises)
                .Where(s => s.UserId == userId && s.IsLatest)
                .FirstOrDefaultAsync();

            if (latestSchedule == null)
            {
                return new ScheduleAnalytics
                {
                    TotalWorkoutsCompleted = 0,
                    TotalWorkoutsPlanned = 0,
                    OverallCompletionRate = 0,
                    CompletionByFocusArea = new Dictionary<string, int>(),
                    MostCompletedExercises = new List<string>(),
                    RecentFeedback = new List<FeedbackSummary>()
                };
            }

            var allDays = latestSchedule.ScheduleDays.ToList();
            var completedDays = allDays.Where(d => d.Completed).ToList();
            var allExercises = allDays.SelectMany(d => d.Exercises).ToList();

            var completionByFocusArea = completedDays
                .GroupBy(d => d.FocusArea)
                .ToDictionary(g => g.Key, g => g.Count());

            var mostCompletedExercises = allExercises
                .Where(e => e.Completed)
                .GroupBy(e => e.ExerciseName)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            var feedbacks = await _context.WorkoutFeedbacks
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Take(10)
                .Select(f => new FeedbackSummary
                {
                    Date = f.CreatedAt,
                    DifficultyRating = f.DifficultyRating,
                    Notes = f.Notes
                })
                .ToListAsync();

            return new ScheduleAnalytics
            {
                TotalWorkoutsCompleted = completedDays.Count,
                TotalWorkoutsPlanned = allDays.Count,
                OverallCompletionRate = allDays.Any() ? (int)((double)completedDays.Count / allDays.Count * 100) : 0,
                CompletionByFocusArea = completionByFocusArea,
                MostCompletedExercises = mostCompletedExercises,
                AverageDifficulty = 0, // TODO: Calculate based on feedback
                RecentFeedback = feedbacks
            };
        }

        // Helper Methods for Adaptive AI

        private string DetermineDifficultyFromFeedback(string currentDifficulty, List<WorkoutFeedback> feedbackHistory)
        {
            if (!feedbackHistory.Any()) return currentDifficulty;

            // Analyze recent feedback (last 5 workouts)
            var recentFeedback = feedbackHistory.Take(5).ToList();
            var tooHardCount = recentFeedback.Count(f => f.DifficultyRating == "too_hard" || f.DifficultyRating == "could_not_complete");
            var tooEasyCount = recentFeedback.Count(f => f.DifficultyRating == "too_easy");

            // If majority say too hard, decrease difficulty
            if (tooHardCount >= 3)
            {
                return currentDifficulty switch
                {
                    "advanced" => "intermediate",
                    "intermediate" => "beginner",
                    _ => "beginner"
                };
            }

            // If majority say too easy, increase difficulty
            if (tooEasyCount >= 3)
            {
                return currentDifficulty switch
                {
                    "beginner" => "intermediate",
                    "intermediate" => "advanced",
                    _ => "advanced"
                };
            }

            return currentDifficulty;
        }

        // TODO: Implement SelectExercisesForDayAdaptive when we add exercise-specific feedback
        // For now, using standard SelectExercisesForDay

        private int GetSetsForDifficultyAdaptive(string difficultyLevel, List<WorkoutFeedback> feedbackHistory)
        {
            var baseSets = GetSetsForDifficulty(difficultyLevel);

            // If recent feedback indicates too hard, reduce sets
            if (feedbackHistory.Take(3).Count(f => f.DifficultyRating == "too_hard") >= 2)
            {
                return Math.Max(2, baseSets - 1);
            }

            // If recent feedback indicates too easy, increase sets
            if (feedbackHistory.Take(3).Count(f => f.DifficultyRating == "too_easy") >= 2)
            {
                return baseSets + 1;
            }

            return baseSets;
        }

        private string BuildRegenerationPrompt(RegenerateScheduleRequest request, List<WorkoutFeedback> feedbackHistory, UserContextResult? userContext)
        {
            var feedbackSummary = feedbackHistory.Any()
                ? string.Join("\n", feedbackHistory.Select(f => $"- Day {f.DayNumber}: {f.DifficultyRating} (Energy: {f.EnergyLevel}/5)"))
                : "No feedback yet";

            var userContextStr = userContext != null
                ? $@"
Health: {userContext.HealthSummary}
Fitness: {userContext.FitnessSummary}
Goals: {userContext.GoalsSummary}"
                : "No user context available";

            return $@"REGENERATE WORKOUT SCHEDULE FROM DAY {request.FromDayNumber}

Reason: {request.Reason}
Additional Notes: {request.AdditionalNotes ?? "None"}

Recent Feedback:
{feedbackSummary}

User Context:
{userContextStr}

IMPORTANT: 
1. Learn from the feedback - adjust difficulty, volume, and exercise selection accordingly
2. Preserve the user's progress and motivation
3. Consider any injuries or limitations mentioned in feedback
4. Adapt the workout to fit their current state and constraints";
        }
    }
}

