using System;

namespace SocialAPI.Domain.Entities
{
    public class PostVote
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string VoteType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
