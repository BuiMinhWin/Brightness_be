using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Domain.Entities
{
    /// <summary>
    /// Shared entities from other services - READ ONLY
    /// These map to existing tables in BrightnessDb
    /// </summary>

    // From AuthenticationAPI
    [Table("users")]
    public class User
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        
        [Column("username")]
        public string UserName { get; set; } = string.Empty;
        
        [Column("full_name")]
        public string? FullName { get; set; }
        
        [Column("date_of_birth")]
        public DateOnly? DateOfBirth { get; set; }
        
        [Column("gender")]
        public string? Gender { get; set; }
        
        [Column("subscription_plan_id")]
        public Guid? SubscriptionPlanId { get; set; }
    }

    // From MeasurementAPI
    [Table("body_measurements")]
    public class BodyMeasurement
    {
        [Column("Id")]
        public Guid Id { get; set; }
        
        [Column("UserId")]
        public Guid UserId { get; set; }
        
        [Column("HeightCm")]
        public decimal? HeightCm { get; set; }
        
        [Column("WeightKg")]
        public decimal? WeightKg { get; set; }
        
        [Column("BodyFatPercent")]
        public decimal? BodyFatPercent { get; set; }
        
        [Column("Goal")]
        public string? Goal { get; set; }
        
        [Column("RecordedAt")]
        public DateTime RecordedAt { get; set; }
    }

    // From BillingAPI
    [Table("subscription_plans")]
    public class SubscriptionPlan
    {
        [Column("Id")]
        public Guid Id { get; set; }
        
        [Column("Name")]
        public string Name { get; set; } = string.Empty;
    }

    // From PersonalCoachAPI
    [Table("user_profiles")]
    public class UserProfile
    {
        [Column("Id")]
        public Guid Id { get; set; }
        
        [Column("UserId")]
        public Guid UserId { get; set; }
        
        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;
        
        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }
        
        [Column("Gender")]
        public string Gender { get; set; } = string.Empty;
        
        [Column("HeightCm")]
        public decimal HeightCm { get; set; }
        
        [Column("CurrentWeightKg")]
        public decimal CurrentWeightKg { get; set; }
        
        [Column("TargetWeightKg")]
        public decimal TargetWeightKg { get; set; }
        
        [Column("FitnessLevel")]
        public string FitnessLevel { get; set; } = string.Empty;
        
        [Column("FitnessGoal")]
        public string FitnessGoal { get; set; } = string.Empty;
    }

    [Table("user_preferences")]
    public class UserPreference
    {
        [Column("Id")]
        public Guid Id { get; set; }
        
        [Column("UserId")]
        public Guid UserId { get; set; }
        
        [Column("AvailableDays")]
        public string AvailableDays { get; set; } = string.Empty; // JSON string
        
        [Column("PreferredWorkoutTime")]
        public string PreferredWorkoutTime { get; set; } = string.Empty;
        
        [Column("WorkoutDurationMinutes")]
        public int WorkoutDurationMinutes { get; set; }
        
        [Column("EquipmentAvailable")]
        public string EquipmentAvailable { get; set; } = string.Empty; // JSON string
        
        [Column("GymAccess")]
        public bool GymAccess { get; set; }
        
        [Column("HomeWorkoutOnly")]
        public bool HomeWorkoutOnly { get; set; }
    }
}

