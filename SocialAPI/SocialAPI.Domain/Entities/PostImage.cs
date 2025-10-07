using System;

namespace SocialAPI.Domain.Entities
{
    public class PostImage
    {
        public Guid Id { get; set; }           // Khóa chính
        public Guid PostId { get; set; }       
        public byte[] ImageData { get; set; } = null!;  // Dữ liệu ảnh dạng BLOB
        public string? FileName { get; set; }  // Tên file gốc (nếu cần)
        public string? ContentType { get; set; } 
        public DateTime CreatedAt { get; set; }

        public Post Post { get; set; } = null!;
    }
}
