using System;

namespace BillingAPI.Application.DTOs
{
    public record InvoiceDTO
    (
        Guid Id,
        Guid? UserExternalId,
        Guid SubscriptionPlanId,
        string InvoiceCode,
        decimal Amount,
        string Currency,
        string Status,
        DateTime CreatedAt,
        DateTime DueDate,
        DateTime? PaidAt,
        DateTime? CancelledAt,
        DateTime? UpdatedAt,
        string? Notes
    );
}
