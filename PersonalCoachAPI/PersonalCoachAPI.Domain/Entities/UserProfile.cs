using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal HeightCm { get; set; }
        public decimal CurrentWeightKg { get; set; }
        public decimal TargetWeightKg { get; set; }
        public string FitnessLevel { get; set; } = "beginner"; // beginner, intermediate, advanced
        public string FitnessGoal { get; set; } = string.Empty; // lose_weight, gain_muscle, maintain, improve_endurance
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

