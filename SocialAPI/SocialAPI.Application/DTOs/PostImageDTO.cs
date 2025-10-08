
using System;

namespace SocialAPI.Application.DTOs
{
    public class PostImageDTO
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
