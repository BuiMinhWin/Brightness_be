using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class UserPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AvailableDays { get; set; } = string.Empty; // JSON array
        public string PreferredWorkoutTime { get; set; } = "morning"; // morning, afternoon, evening
        public int WorkoutDurationMinutes { get; set; } = 45;
        public string EquipmentAvailable { get; set; } = string.Empty; // JSON array
        public bool GymAccess { get; set; }
        public bool HomeWorkoutOnly { get; set; }
        public string DietaryRestrictions { get; set; } = string.Empty; // JSON array
        public string DislikedExercises { get; set; } = string.Empty; // JSON array
        public string FavoriteExercises { get; set; } = string.Empty; // JSON array
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

