using System;

namespace ChallengeAPI.Domain.Entities
{
    public class ChallengeRule
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public string? HowToParticipate { get; set; }
        public string? RuleText { get; set; }
    }
}
