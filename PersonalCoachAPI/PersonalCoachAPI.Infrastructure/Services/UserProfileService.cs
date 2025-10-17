using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using PersonalCoachAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _profileRepository;
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(
            IUserProfileRepository profileRepository,
            PersonalCoachDbContext context,
            ILogger<UserProfileService> logger)
        {
            _profileRepository = profileRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(Guid userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            var preference = await _profileRepository.GetPreferenceByUserIdAsync(userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile not found");
            }

            // Get user info from AuthenticationAPI table
            var authUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            return new UserProfileResponse
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = authUser?.FullName ?? profile.FullName,
                DateOfBirth = authUser?.DateOfBirth != null ? DateTime.SpecifyKind(authUser.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc) : profile.DateOfBirth,
                Gender = authUser?.Gender ?? profile.Gender,
                HeightCm = profile.HeightCm,
                CurrentWeightKg = profile.CurrentWeightKg,
                TargetWeightKg = profile.TargetWeightKg,
                FitnessLevel = profile.FitnessLevel,
                FitnessGoal = profile.FitnessGoal,
                
                AvailableDays = preference != null ? JsonConvert.DeserializeObject<List<string>>(preference.AvailableDays) ?? new List<string>() : new List<string>(),
                PreferredWorkoutTime = preference?.PreferredWorkoutTime ?? "morning",
                WorkoutDurationMinutes = preference?.WorkoutDurationMinutes ?? 45,
                EquipmentAvailable = preference != null ? JsonConvert.DeserializeObject<List<string>>(preference.EquipmentAvailable) ?? new List<string>() : new List<string>(),
                GymAccess = preference?.GymAccess ?? false,
                HomeWorkoutOnly = preference?.HomeWorkoutOnly ?? false,
                DietaryRestrictions = preference != null ? JsonConvert.DeserializeObject<List<string>>(preference.DietaryRestrictions) ?? new List<string>() : new List<string>(),
                DislikedExercises = preference != null ? JsonConvert.DeserializeObject<List<string>>(preference.DislikedExercises) ?? new List<string>() : new List<string>(),
                FavoriteExercises = preference != null ? JsonConvert.DeserializeObject<List<string>>(preference.FavoriteExercises) ?? new List<string>() : new List<string>(),
                
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        public async Task<UserProfileResponse> CreateOrUpdateProfileAsync(CreateUserProfileRequest request)
        {
            _logger.LogInformation("Creating/updating profile for user {UserId}", request.UserId);

            // Get or create user info from AuthenticationAPI
            var authUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);

            // Check if profile exists
            var existingProfile = await _profileRepository.GetByUserIdAsync(request.UserId);
            var existingPreference = await _profileRepository.GetPreferenceByUserIdAsync(request.UserId);

            UserProfile profile;
            if (existingProfile == null)
            {
                // Create new profile
                profile = new UserProfile
                {
                    UserId = request.UserId,
                    FullName = authUser?.FullName ?? "",
                    DateOfBirth = authUser?.DateOfBirth != null ? DateTime.SpecifyKind(authUser.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc) : null,
                    Gender = authUser?.Gender ?? "",
                    HeightCm = request.HeightCm,
                    CurrentWeightKg = request.CurrentWeightKg,
                    TargetWeightKg = request.TargetWeightKg,
                    FitnessLevel = request.FitnessLevel,
                    FitnessGoal = request.FitnessGoal
                };
                profile = await _profileRepository.CreateAsync(profile);
            }
            else
            {
                // Update existing profile
                existingProfile.HeightCm = request.HeightCm;
                existingProfile.CurrentWeightKg = request.CurrentWeightKg;
                existingProfile.TargetWeightKg = request.TargetWeightKg;
                existingProfile.FitnessLevel = request.FitnessLevel;
                existingProfile.FitnessGoal = request.FitnessGoal;
                
                profile = await _profileRepository.UpdateAsync(existingProfile);
            }

            // Handle preferences
            UserPreference preference;
            if (existingPreference == null)
            {
                // Create new preference
                preference = new UserPreference
                {
                    UserId = request.UserId,
                    AvailableDays = JsonConvert.SerializeObject(request.AvailableDays),
                    PreferredWorkoutTime = request.PreferredWorkoutTime,
                    WorkoutDurationMinutes = request.WorkoutDurationMinutes,
                    EquipmentAvailable = JsonConvert.SerializeObject(request.EquipmentAvailable),
                    GymAccess = request.GymAccess,
                    HomeWorkoutOnly = request.HomeWorkoutOnly,
                    DietaryRestrictions = JsonConvert.SerializeObject(request.DietaryRestrictions),
                    DislikedExercises = JsonConvert.SerializeObject(request.DislikedExercises),
                    FavoriteExercises = JsonConvert.SerializeObject(request.FavoriteExercises)
                };
                preference = await _profileRepository.CreatePreferenceAsync(preference);
            }
            else
            {
                // Update existing preference
                existingPreference.AvailableDays = JsonConvert.SerializeObject(request.AvailableDays);
                existingPreference.PreferredWorkoutTime = request.PreferredWorkoutTime;
                existingPreference.WorkoutDurationMinutes = request.WorkoutDurationMinutes;
                existingPreference.EquipmentAvailable = JsonConvert.SerializeObject(request.EquipmentAvailable);
                existingPreference.GymAccess = request.GymAccess;
                existingPreference.HomeWorkoutOnly = request.HomeWorkoutOnly;
                existingPreference.DietaryRestrictions = JsonConvert.SerializeObject(request.DietaryRestrictions);
                existingPreference.DislikedExercises = JsonConvert.SerializeObject(request.DislikedExercises);
                existingPreference.FavoriteExercises = JsonConvert.SerializeObject(request.FavoriteExercises);
                
                preference = await _profileRepository.UpdatePreferenceAsync(existingPreference);
            }

            // Note: User context snapshot will be created automatically by RAGService
            // when GetUserContextAsync is called during workout generation

            _logger.LogInformation("Profile and preferences saved for user {UserId}", request.UserId);

            return await GetUserProfileAsync(request.UserId);
        }
    }
}

