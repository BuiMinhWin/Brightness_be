using System;

namespace BillingAPI.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid? UserExternalId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
