using System;

namespace AuthenticationAPI.Application.DTOs
{
    public record GetUserDTO
    (
        Guid Id,
        string Email,
        string UserName,
        string? FullName,
        string? PhoneNumber,
        string? Gender,
        string? ProfilePicture,
        DateOnly? DateOfBirth,
        string Role,
        string Status,
        DateTime CreatedAt,
        Guid? SubscriptionPlanId,
        string? PaymentCustomerRef
    );
}
