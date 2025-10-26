using System;
using System.Collections.Generic;

namespace SocialAPI.Domain.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Visibility { get; set; } = "public";
        public string Status { get; set; } = "draft";
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int ReactionCount { get; set; } = 0;
        public int DislikeCount { get; set; } = 0;

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>(); // Ch? gi? l?i d�ng n�y
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    }
}