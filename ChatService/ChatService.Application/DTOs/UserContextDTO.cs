using System;
using System.Collections.Generic;

namespace ChatService.Application.DTOs
{
    public record UserContextDTO
    {
        public Guid UserId { get; init; }
        public string? FullName { get; init; }
        public string? Email { get; init; }
        public int? Age { get; init; }
        public string? Gender { get; init; }
        public string? SubscriptionPlan { get; init; }
        
        // Measurement data
        public decimal? CurrentWeight { get; init; }
        public decimal? Height { get; init; }
        public decimal? BodyFat { get; init; }
        
        // Goals
        public List<string> Goals { get; init; } = new();
        
        // Fitness profile data
        public string? FitnessLevel { get; init; } // beginner, intermediate, advanced
        public decimal? TargetWeight { get; init; }
        public string? PreferredWorkoutTime { get; init; } // morning, afternoon, evening
        public int? WorkoutDurationMinutes { get; init; }
        public bool? GymAccess { get; init; }
        public bool? HomeWorkoutOnly { get; init; }
        public string? AvailableDays { get; init; } // JSON string
        public string? EquipmentAvailable { get; init; } // JSON string
        
        // Recent activity
        public List<string> RecentWorkouts { get; init; } = new();
        public List<string> RecentNutrition { get; init; } = new();
        
        // Conversation summary
        public string? ConversationSummary { get; init; }
    }
}

