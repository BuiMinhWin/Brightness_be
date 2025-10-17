using System;

namespace WorkoutAPI.Application.DTOs
{
    public record WorkoutSessionDTO
    (
        Guid Id,
        Guid WorkoutPlanId,
        DateOnly SessionDate,
        int? DurationMinutes,
        string? Notes,
        bool Completed,
        DateTime CreatedAt
    );
}
