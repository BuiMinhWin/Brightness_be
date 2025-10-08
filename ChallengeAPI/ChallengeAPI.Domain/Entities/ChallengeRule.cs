using System;

namespace ChallengeAPI.Domain.Entities
{
    public class ChallengeRule
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public List<string>? HowToParticipate { get; set; }
        public List<string>? RuleText { get; set; }

        public Challenge Challenge { get; set; } = null!;
    }
}
