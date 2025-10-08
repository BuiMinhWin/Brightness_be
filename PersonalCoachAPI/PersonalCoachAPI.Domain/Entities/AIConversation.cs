using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class AIConversation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
        public string MessageType { get; set; } = string.Empty; // user, assistant, system
        public string MessageContent { get; set; } = string.Empty;
        public string ContextUsed { get; set; } = string.Empty; // JSON - what data was used
        public string RagResults { get; set; } = string.Empty; // JSON - retrieved knowledge base items
        public DateTime CreatedAt { get; set; }
    }
}

