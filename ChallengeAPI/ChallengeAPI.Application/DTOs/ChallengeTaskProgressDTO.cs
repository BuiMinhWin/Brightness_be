using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeTaskProgressDTO
    (
        Guid Id = default,
        Guid ParticipantId = default,
        Guid ChallengeTaskId = default,
        DateOnly? TargetDate = null,
        string? Status = null,
        string? ValidationData = null,
        decimal? PointsEarned = null,
        DateTime? CompletedAt = null
    );
}
