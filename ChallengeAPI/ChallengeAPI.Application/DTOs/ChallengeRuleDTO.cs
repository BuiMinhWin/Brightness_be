using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeRuleDTO
    (
        Guid Id,
        Guid ChallengeId,
        string? HowToParticipate,
        string? RuleText
    );
}
