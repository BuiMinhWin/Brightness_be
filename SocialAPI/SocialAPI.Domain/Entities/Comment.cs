using System;

namespace SocialAPI.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; } // Denormalized user name for resilience
        public Guid? ParentCommentId { get; set; }
        public string Content { get; set; } = null!;
        public string Status { get; set; } = "visible";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReactionCount { get; set; } = 0;
    }
}
