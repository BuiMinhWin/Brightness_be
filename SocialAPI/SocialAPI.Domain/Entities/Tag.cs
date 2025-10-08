using System;
using System.Collections.Generic;

namespace SocialAPI.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}