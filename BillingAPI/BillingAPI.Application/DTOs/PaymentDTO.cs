using System;

namespace BillingAPI.Application.DTOs
{
    public record PaymentDTO
    (
        Guid Id,
        Guid SubscriptionId,
        Guid? UserExternalId,
        decimal Amount,
        string Currency,
        string? PaymentMethod,
        string PaymentStatus,
        string? TransactionId,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
