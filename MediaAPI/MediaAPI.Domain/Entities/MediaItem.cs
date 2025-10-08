using System.Collections.Generic;

namespace MediaAPI.Domain.Entities
{
    public class MediaItem
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Url { get; set; } = null!;
        public string? Thumbnail { get; set; }
        public string? Category { get; set; }
        public string? Duration { get; set; }
        public string? Platform { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int Views { get; set; } = 0;
        public int Likes { get; set; } = 0;

        public ICollection<UserLike> UserLikes { get; set; } = new List<UserLike>();
        public ICollection<UserView> UserViews { get; set; } = new List<UserView>();
    }
}