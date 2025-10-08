using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class UserHealthMetric
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime MeasurementDate { get; set; }
        public decimal WeightKg { get; set; }
        public decimal MuscleMassKg { get; set; }
        public decimal BodyFatPercentage { get; set; }
        public decimal BodyFatKg { get; set; }
        public int BmrKcal { get; set; }
        public decimal WaterPercentage { get; set; }
        public decimal BoneMassKg { get; set; }
        public int VisceralFatLevel { get; set; }
        public int MetabolicAge { get; set; }
        public string Source { get; set; } = string.Empty; // inbody, manual, smart_scale
        public string RawDataJson { get; set; } = string.Empty; // Full InBody report
        public DateTime CreatedAt { get; set; }
    }
}

