using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Domain.Entities
{
    [Table("conversation_summaries")]
    public class ConversationSummary
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("session_id")]
        public Guid SessionId { get; set; }
        
        [Column("summary_text")]
        public string SummaryText { get; set; } = null!;
        
        [Column("message_count")]
        public int MessageCount { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public ChatSession? Session { get; set; }
    }
}

