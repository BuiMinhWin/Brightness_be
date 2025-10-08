using System;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeRuleDTO
    (
        Guid Id,
        Guid ChallengeId,
        List<string>? HowToParticipate,
        List<string>? RuleText
    );
}
