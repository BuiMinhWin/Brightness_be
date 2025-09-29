using System;

namespace BillingAPI.Application.DTOs
{
    public record SubscriptionPlanDTO
    (
        Guid Id,
        string Name,
        decimal Price,
        string Currency,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
