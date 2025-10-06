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
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>(); // Ch? gi? l?i dòng này
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    }
}