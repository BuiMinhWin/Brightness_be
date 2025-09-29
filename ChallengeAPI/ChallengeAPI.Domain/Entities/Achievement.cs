using System;

namespace ChallengeAPI.Domain.Entities
{
    public class Achievement
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ChallengeId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
