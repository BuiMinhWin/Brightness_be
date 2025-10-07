using System;
using BillingAPI.Domain.Enums;

namespace BillingAPI.Application.Responses
{
    public record PaymentStatusResponse
    (
        Guid PaymentId,
        Guid InvoiceId,
        PaymentGatewayType Gateway,
        string PaymentStatus,
        DateTime? PaidAt,
        string? TransactionId
    );
}
