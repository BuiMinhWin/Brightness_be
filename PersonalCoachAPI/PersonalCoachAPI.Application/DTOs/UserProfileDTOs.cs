using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Application.DTOs
{
    public class CreateUserProfileRequest
    {
        public Guid UserId { get; set; }
        
        // UserProfile fields
        public decimal HeightCm { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal TargetWeightKg { get; set; }
        public string FitnessLevel { get; set; } = "beginner"; // beginner, intermediate, advanced
        public string FitnessGoal { get; set; } = string.Empty; // lose_weight, gain_muscle, maintain, improve_endurance
        
        // UserPreference fields
        public List<string> AvailableDays { get; set; } = new List<string>();
        public string PreferredWorkoutTime { get; set; } = "morning"; // morning, afternoon, evening
        public int WorkoutDurationMinutes { get; set; } = 45;
        public List<string> EquipmentAvailable { get; set; } = new List<string>();
        public bool GymAccess { get; set; }
        public bool HomeWorkoutOnly { get; set; }
        public List<string> DietaryRestrictions { get; set; } = new List<string>();
        public List<string> DislikedExercises { get; set; } = new List<string>();
        public List<string> FavoriteExercises { get; set; } = new List<string>();
        
        // Optional: Medical conditions (stored as text)
        public string MedicalConditions { get; set; } = string.Empty;
    }

    public class UpdateUserProfileRequest
    {
        public decimal? HeightCm { get; set; }
        public decimal? CurrentWeightKg { get; set; }
        public decimal? TargetWeightKg { get; set; }
        public string? FitnessLevel { get; set; }
        public string? FitnessGoal { get; set; }
        
        public List<string>? AvailableDays { get; set; }
        public string? PreferredWorkoutTime { get; set; }
        public int? WorkoutDurationMinutes { get; set; }
        public List<string>? EquipmentAvailable { get; set; }
        public bool? GymAccess { get; set; }
        public bool? HomeWorkoutOnly { get; set; }
        public List<string>? DietaryRestrictions { get; set; }
        public List<string>? DislikedExercises { get; set; }
        public List<string>? FavoriteExercises { get; set; }
        public string? MedicalConditions { get; set; }
    }

    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        
        // UserProfile
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal HeightCm { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal TargetWeightKg { get; set; }
        public string FitnessLevel { get; set; } = string.Empty;
        public string FitnessGoal { get; set; } = string.Empty;
        
        // UserPreference
        public List<string> AvailableDays { get; set; } = new List<string>();
        public string PreferredWorkoutTime { get; set; } = string.Empty;
        public int WorkoutDurationMinutes { get; set; }
        public List<string> EquipmentAvailable { get; set; } = new List<string>();
        public bool GymAccess { get; set; }
        public bool HomeWorkoutOnly { get; set; }
        public List<string> DietaryRestrictions { get; set; } = new List<string>();
        public List<string> DislikedExercises { get; set; } = new List<string>();
        public List<string> FavoriteExercises { get; set; } = new List<string>();
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

