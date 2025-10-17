using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Infrastructure.Data;
using ChatService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly ChatDbContext _context;
        private readonly IConversationSummaryRepository _summaryRepository;

        public UserContextService(ChatDbContext context, IConversationSummaryRepository summaryRepository)
        {
            _context = context;
            _summaryRepository = summaryRepository;
        }

        public async Task<UserContextDTO> BuildUserContextAsync(Guid userId, Guid? sessionId = null)
        {
            // Fetch user data
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.FullName, u.Email, u.Gender, u.DateOfBirth, u.SubscriptionPlanId })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not found");
            }

            // Get subscription plan name
            string? subscriptionPlan = null;
            if (user.SubscriptionPlanId.HasValue)
            {
                subscriptionPlan = await _context.SubscriptionPlans
                    .Where(sp => sp.Id == user.SubscriptionPlanId.Value)
                    .Select(sp => sp.Name)
                    .FirstOrDefaultAsync();
            }

            // Calculate age
            int? age = null;
            if (user.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                age = today.Year - user.DateOfBirth.Value.Year;
                if (today < user.DateOfBirth.Value.AddYears(age.Value))
                {
                    age--;
                }
            }

            // Get fitness profile from PersonalCoachAPI (preferred source)
            var fitnessProfile = await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            var userPreferences = await _context.UserPreferences
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            // Fallback to body measurements if no fitness profile
            var latestMeasurement = await _context.BodyMeasurements
                .Where(bm => bm.UserId == userId)
                .OrderByDescending(bm => bm.RecordedAt)
                .Select(bm => new { bm.WeightKg, bm.HeightCm, bm.BodyFatPercent, bm.Goal })
                .FirstOrDefaultAsync();

            // Build user goals list
            var goals = new List<string>();
            if (!string.IsNullOrWhiteSpace(fitnessProfile?.FitnessGoal))
            {
                goals.Add(fitnessProfile.FitnessGoal);
            }
            else if (!string.IsNullOrWhiteSpace(latestMeasurement?.Goal))
            {
                goals.Add(latestMeasurement.Goal);
            }

            // Get conversation summary if session exists
            string? conversationSummary = null;
            if (sessionId.HasValue)
            {
                var summary = await _summaryRepository.GetLatestBySessionIdAsync(sessionId.Value);
                conversationSummary = summary?.SummaryText;
            }

            // Prefer fitness profile data over body measurements
            var height = fitnessProfile?.HeightCm ?? latestMeasurement?.HeightCm;
            var currentWeight = fitnessProfile?.CurrentWeightKg ?? latestMeasurement?.WeightKg;

            return new UserContextDTO
            {
                UserId = userId,
                FullName = user.FullName,
                Email = user.Email,
                Age = age,
                Gender = user.Gender,
                SubscriptionPlan = subscriptionPlan ?? "Free",
                CurrentWeight = currentWeight,
                Height = height,
                BodyFat = latestMeasurement?.BodyFatPercent,
                Goals = goals,
                RecentWorkouts = new List<string>(), // Can be expanded later
                RecentNutrition = new List<string>(), // Can be expanded later
                ConversationSummary = conversationSummary,
                // Add fitness profile specific data
                FitnessLevel = fitnessProfile?.FitnessLevel,
                TargetWeight = fitnessProfile?.TargetWeightKg,
                PreferredWorkoutTime = userPreferences?.PreferredWorkoutTime,
                WorkoutDurationMinutes = userPreferences?.WorkoutDurationMinutes,
                GymAccess = userPreferences?.GymAccess,
                HomeWorkoutOnly = userPreferences?.HomeWorkoutOnly,
                AvailableDays = userPreferences?.AvailableDays,
                EquipmentAvailable = userPreferences?.EquipmentAvailable
            };
        }
    }
}

