using System;
using BillingAPI.Domain.Enums;

namespace BillingAPI.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid? UserExternalId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public PaymentGatewayType Gateway { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string? TransactionId { get; set; }
        public string? CheckoutUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RawGatewayPayload { get; set; }

        public Invoice Invoice { get; set; } = null!;
    }
}
