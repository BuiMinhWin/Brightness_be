using System;
using System.Collections.Generic;

namespace BillingAPI.Domain.Entities
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "VND";
        public int DurationDays { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
