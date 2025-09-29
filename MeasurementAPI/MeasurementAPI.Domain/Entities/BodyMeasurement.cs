using System;

namespace MeasurementAPI.Domain.Entities
{
    public class BodyMeasurement
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? BodyFatPercent { get; set; }
        public decimal? WaistCm { get; set; }
        public decimal? ChestCm { get; set; }
        public decimal? HipCm { get; set; }
        public string? ActivityLevel { get; set; }
        public int? WorkoutDaysPerWeek { get; set; }
        public string? Goal { get; set; }
        public string? HealthNote { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
