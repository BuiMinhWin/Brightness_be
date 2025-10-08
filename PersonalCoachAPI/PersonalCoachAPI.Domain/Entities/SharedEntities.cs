using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalCoachAPI.Domain.Entities
{
    /// <summary>
    /// Shared entities from other services - READ ONLY
    /// These map to existing tables in BrightnessDb
    /// </summary>

    // From AuthenticationAPI
    [Table("users")]
    public class AuthUser
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
        
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }
        
        [Column("subscription_plan_id")]
        public Guid? SubscriptionPlanId { get; set; }
        
        [Column("role")]
        public string Role { get; set; } = string.Empty;
        
        [Column("status")]
        public string Status { get; set; } = string.Empty;
        
        [Column("create_at")]
        public DateTime CreatedAt { get; set; }
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
        
        [Column("WaistCm")]
        public decimal? WaistCm { get; set; }
        
        [Column("ChestCm")]
        public decimal? ChestCm { get; set; }
        
        [Column("HipCm")]
        public decimal? HipCm { get; set; }
        
        [Column("ActivityLevel")]
        public string? ActivityLevel { get; set; }
        
        [Column("WorkoutDaysPerWeek")]
        public int? WorkoutDaysPerWeek { get; set; }
        
        [Column("Goal")]
        public string? Goal { get; set; }
        
        [Column("HealthNote")]
        public string? HealthNote { get; set; }
        
        [Column("RecordedAt")]
        public DateTime RecordedAt { get; set; }
    }

    // From AIOCRService
    [Table("ocr_requests")]
    public class OCRRequest
    {
        [Column("Id")]
        public Guid Id { get; set; }
        
        [Column("UserId")]
        public Guid UserId { get; set; }
        
        [Column("FileName")]
        public string FileName { get; set; } = string.Empty;
        
        [Column("Status")]
        public string Status { get; set; } = string.Empty;
        
        [Column("Type")]
        public int Type { get; set; }
        
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("processed_data")]
    public class ProcessedData
    {
        [Column("Id")]
        public Guid Id { get; set; }
        
        [Column("OCRResultId")]
        public Guid? OCRResultId { get; set; }  // Nullable - for direct AI vision processing
        
        [Column("UserId")]
        public Guid UserId { get; set; }
        
        [Column("DataType")]
        public string DataType { get; set; } = string.Empty;
        
        [Column("ProcessedContent")]
        public string ProcessedContent { get; set; } = string.Empty;
        
        [Column("Summary")]
        public string Summary { get; set; } = string.Empty;
        
        [Column("Insights")]
        public string Insights { get; set; } = string.Empty;
        
        [Column("IsLatest")]
        public bool IsLatest { get; set; } = true;
        
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}

