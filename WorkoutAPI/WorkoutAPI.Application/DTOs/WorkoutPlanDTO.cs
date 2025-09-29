using System;

namespace WorkoutAPI.Application.DTOs
{
    public record WorkoutPlanDTO
    (
        Guid Id,
        Guid UserId,
        string Name,
        DateOnly? StartDate,
        DateOnly? EndDate,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
