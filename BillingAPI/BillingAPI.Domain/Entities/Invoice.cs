using System;
using System.Collections.Generic;

namespace BillingAPI.Domain.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid? UserExternalId { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public string InvoiceCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }

        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
