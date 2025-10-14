using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Domain.Entities
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("session_id")]
        public Guid SessionId { get; set; }
        
        [Column("user_id")]
        public Guid UserId { get; set; }
        
        [Column("role")]
        public string Role { get; set; } = null!; // "user" or "ai"
        
        [Column("content")]
        public string Content { get; set; } = null!;
        
        [Column("token_count")]
        public int? TokenCount { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public ChatSession? Session { get; set; }
    }
}

