using System;
using BillingAPI.Domain.Enums;

namespace BillingAPI.Application.Responses
{
    public record PaymentSessionResponse
    (
        Guid PaymentId,
        Guid InvoiceId,
        PaymentGatewayType Gateway,
        string PaymentUrl,
        DateTime? ExpiredAt,
        string? TransactionId
    );
}
