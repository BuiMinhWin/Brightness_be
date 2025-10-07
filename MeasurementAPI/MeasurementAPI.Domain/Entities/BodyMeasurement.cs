using System;
using MeasurementAPI.Domain.Enums;

namespace MeasurementAPI.Domain.Entities
{
    public class BodyMeasurement
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? DeviceId { get; set; }
        public MeasurementSourceType Source { get; set; } = MeasurementSourceType.Manual;
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? BodyFatPercent { get; set; }
        public decimal? SkeletalMuscleMassKg { get; set; }
        public decimal? TotalBodyWaterPercent { get; set; }
        public decimal? BasalMetabolicRate { get; set; }
        public decimal? WaistCm { get; set; }
        public decimal? ChestCm { get; set; }
        public decimal? HipCm { get; set; }
        public string? ActivityLevel { get; set; }
        public int? WorkoutDaysPerWeek { get; set; }
        public string? Goal { get; set; }
        public string? HealthNote { get; set; }
        public string? RawData { get; set; }
        public DateTime RecordedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SyncedAt { get; set; }

        public MeasurementDevice? Device { get; set; }
    }
}
