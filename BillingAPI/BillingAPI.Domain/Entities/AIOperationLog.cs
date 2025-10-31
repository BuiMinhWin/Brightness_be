using System;

namespace BillingAPI.Domain.Entities
{
    public class AIOperationLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CreditPoolId { get; set; }
        public string ServiceName { get; set; } = null!;
        public string OperationType { get; set; } = null!; // "chat_message", "schedule_generation", etc.
        public string Model { get; set; } = null!;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public int TokenCost { get; set; } // Deducted from pool
        public decimal EstimatedMonetaryCost { get; set; }
        public string? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public AICreditPool CreditPool { get; set; } = null!;
    }
}

