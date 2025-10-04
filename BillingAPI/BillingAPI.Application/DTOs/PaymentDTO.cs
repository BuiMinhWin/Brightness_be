using System;
using BillingAPI.Domain.Enums;

namespace BillingAPI.Application.DTOs
{
    public record PaymentDTO
    (
        Guid Id,
        Guid InvoiceId,
        Guid? UserExternalId,
        decimal Amount,
        string Currency,
        PaymentGatewayType Gateway,
        string PaymentStatus,
        string? TransactionId,
        string? CheckoutUrl,
        DateTime CreatedAt,
        DateTime? ExpiredAt,
        DateTime? UpdatedAt
    );
}
