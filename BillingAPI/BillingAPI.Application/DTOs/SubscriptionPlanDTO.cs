using System;

namespace BillingAPI.Application.DTOs
{
    public record SubscriptionPlanDTO
    (
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        string Currency,
        int DurationDays,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
