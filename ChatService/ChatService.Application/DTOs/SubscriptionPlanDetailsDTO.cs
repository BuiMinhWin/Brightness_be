using System;

namespace ChatService.Application.DTOs
{
    /// <summary>
    /// DTO for subscription plan details from BillingAPI
    /// </summary>
    public record SubscriptionPlanDetailsDTO
    (
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        string Currency,
        int DurationDays,
        bool IsActive
    );
}


