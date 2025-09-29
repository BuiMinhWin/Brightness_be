using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeTaskDTO
    (
        Guid Id,
        Guid ChallengeId,
        string Name,
        string? Description,
        string? TaskType,
        string? FrequencyRule,
        decimal? Points,
        bool IsRequired,
        string? ValidationRule,
        DateOnly? StartDate,
        DateOnly? EndDate
    );
}
