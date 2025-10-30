using System;

namespace BillingAPI.Domain.Entities
{
    public class CreditCostConfiguration
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = null!;
        public string OperationType { get; set; } = null!;
        public decimal BaseTokenMultiplier { get; set; } // e.g., 1.0 for chat, 5.0 for schedule
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

