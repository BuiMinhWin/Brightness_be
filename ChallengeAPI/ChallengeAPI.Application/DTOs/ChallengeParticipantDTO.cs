using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeParticipantDTO
    (
        Guid Id,
        Guid ChallengeId,
        Guid UserId,
        DateTime JoinedAt,
        string Status,
        decimal? TotalPoints,
        decimal? PercentageCompleted,
        int? CurrentStreak,
        int? LongestStreak
    );
}
