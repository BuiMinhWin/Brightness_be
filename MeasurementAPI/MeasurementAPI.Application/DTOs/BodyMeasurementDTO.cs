using System;
using MeasurementAPI.Domain.Enums;

namespace MeasurementAPI.Application.DTOs
{
    public record BodyMeasurementDTO
    (
        Guid Id,
        Guid UserId,
        Guid? DeviceId,
        MeasurementSourceType Source,
        decimal? HeightCm,
        decimal? WeightKg,
        decimal? BodyFatPercent,
        decimal? SkeletalMuscleMassKg,
        decimal? TotalBodyWaterPercent,
        decimal? BasalMetabolicRate,
        decimal? WaistCm,
        decimal? ChestCm,
        decimal? HipCm,
        string? ActivityLevel,
        int? WorkoutDaysPerWeek,
        string? Goal,
        string? HealthNote,
        string? RawData,
        DateTime RecordedAt,
        DateTime CreatedAt,
        DateTime? SyncedAt
    );
}
