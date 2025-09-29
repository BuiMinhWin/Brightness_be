using System;

namespace MeasurementAPI.Application.DTOs
{
    public record BodyMeasurementDTO
    (
        Guid Id,
        Guid UserId,
        decimal? HeightCm,
        decimal? WeightKg,
        decimal? BodyFatPercent,
        decimal? WaistCm,
        decimal? ChestCm,
        decimal? HipCm,
        string? ActivityLevel,
        int? WorkoutDaysPerWeek,
        string? Goal,
        string? HealthNote,
        DateTime RecordedAt
    );
}
