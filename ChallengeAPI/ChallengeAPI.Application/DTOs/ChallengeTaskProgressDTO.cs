using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeTaskProgressDTO
    (
        Guid Id,
        Guid ParticipantId,
        Guid ChallengeTaskId,
        DateOnly? TargetDate,
        string Status,
        string? ValidationData,
        decimal? PointsEarned,
        DateTime? CompletedAt
    );
}
