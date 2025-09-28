using System;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Application.DTOs
{
    public record UserDTO
    (
        Guid? Id,
        [Required, EmailAddress] string Email,
        [Required] string UserName,
        [Required] string Password,
        string? FullName,
        string? PhoneNumber,
        string? Gender,
        string? ProfilePicture,
        DateOnly? DateOfBirth,
        [Required] string Role,
        [Required] string Status,
        Guid? SubscriptionPlanId,
        string? PaymentCustomerRef
    );
}
