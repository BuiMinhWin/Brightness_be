using System;

namespace ChallengeAPI.Application.DTOs
{
    public record AchievementDTO
    (
        Guid Id,
        Guid UserId,
        Guid? ChallengeId,
        string Code,
        string Name,
        string? Description,
        DateTime CreatedAt
    );
}
