using System;

namespace AuthenticationAPI.Application.DTOs
{
    public record UpdateUserDTO
    (
        string? Email,
        string? UserName,
        string? FullName,
        string? PhoneNumber,
        string? Gender,
        string? ProfilePicture,
        DateOnly? DateOfBirth,
        string? Role,
        string? Status,
        Guid? SubscriptionPlanId,
        string? PaymentCustomerRef,
        string? Password
    );
}
