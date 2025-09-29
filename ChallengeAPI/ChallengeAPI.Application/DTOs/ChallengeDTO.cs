using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeDTO
    (
        Guid Id,
        Guid HostUserId,
        string Title,
        string? Description,
        DateTime? StartAt,
        DateTime? EndAt,
        string Status,
        int? MaxParticipants,
        decimal? PrizePool,
        string? Category,
        string? TaskSummary,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
