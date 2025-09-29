using System;

namespace MediaAPI.Domain.Entities
{
    public class MediaItem
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Url { get; set; } = null!;
        public string? Platform { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
