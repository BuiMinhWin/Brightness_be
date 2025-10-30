using System;

namespace BillingAPI.Domain.Entities
{
    public class AICreditPool
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ServiceName { get; set; } = null!; // "chat" or "schedule"
        public string SubscriptionTier { get; set; } = null!; // "free", "basic", "premium"
        public int MonthlyTokenLimit { get; set; }
        public int RemainingTokens { get; set; }
        public DateTime CycleStartDate { get; set; }
        public DateTime CycleEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<AIOperationLog> OperationLogs { get; set; } = new List<AIOperationLog>();
    }
}

