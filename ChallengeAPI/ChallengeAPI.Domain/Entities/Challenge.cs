using System;
using System.Collections.Generic;

namespace ChallengeAPI.Domain.Entities
{
    public class Challenge
    {
        public Guid Id { get; set; }
        public Guid HostUserId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; } = "draft";
        public int? MaxParticipants { get; set; }
        public decimal? PrizePool { get; set; }
        public string? Category { get; set; }
        public string? TaskSummary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ChallengeRule> Rules { get; set; } = new List<ChallengeRule>();
        public ICollection<ChallengeTask> Tasks { get; set; } = new List<ChallengeTask>();
    }
}
