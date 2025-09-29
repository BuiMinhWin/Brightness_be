using System;

namespace ChallengeAPI.Domain.Entities
{
    public class ChallengeTaskProgress
    {
        public Guid Id { get; set; }
        public Guid ParticipantId { get; set; }
        public Guid ChallengeTaskId { get; set; }
        public DateOnly? TargetDate { get; set; }
        public string Status { get; set; } = "pending";
        public string? ValidationData { get; set; }
        public decimal? PointsEarned { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
