using System;

namespace ChallengeAPI.Domain.Entities
{
    public class ChallengeParticipant
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public string Status { get; set; } = "pending";
        public decimal? TotalPoints { get; set; }
        public decimal? PercentageCompleted { get; set; }
        public int? CurrentStreak { get; set; }
        public int? LongestStreak { get; set; }
    }
}
