using System;
using System.ComponentModel.DataAnnotations;
using MeasurementAPI.Domain.Enums;

namespace MeasurementAPI.Application.Requests
{
    public class CreateBodyMeasurementRequest
    {
        [Required]
        public Guid UserId { get; set; }

        public Guid? DeviceId { get; set; }

        public MeasurementSourceType Source { get; set; } = MeasurementSourceType.Manual;

        [Range(50, 300)]
        public decimal? HeightCm { get; set; }

        [Range(20, 500)]
        public decimal? WeightKg { get; set; }

        [Range(0, 100)]
        public decimal? BodyFatPercent { get; set; }

        [Range(0, 100)]
        public decimal? SkeletalMuscleMassKg { get; set; }

        [Range(0, 100)]
        public decimal? TotalBodyWaterPercent { get; set; }

        [Range(0, 10000)]
        public decimal? BasalMetabolicRate { get; set; }

        [Range(0, 400)]
        public decimal? WaistCm { get; set; }

        [Range(0, 400)]
        public decimal? ChestCm { get; set; }

        [Range(0, 400)]
        public decimal? HipCm { get; set; }

        [MaxLength(128)]
        public string? ActivityLevel { get; set; }

        [Range(0, 7)]
        public int? WorkoutDaysPerWeek { get; set; }

        [MaxLength(256)]
        public string? Goal { get; set; }

        [MaxLength(1024)]
        public string? HealthNote { get; set; }

        public string? RawData { get; set; }

        [Required]
        public DateTime RecordedAt { get; set; }
    }
}
