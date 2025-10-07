using System;

namespace BillingAPI.Application.Responses
{
    public record GatewayPaymentResult
    (
        string PaymentUrl,
        DateTime? ExpiredAt,
        string? TransactionId
    );
}
