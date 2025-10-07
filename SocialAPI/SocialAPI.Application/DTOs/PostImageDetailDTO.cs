
using System;

namespace SocialAPI.Application.DTOs
{
    public class PostImageDetailDTO
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public byte[] ImageData { get; set; } = null!;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
