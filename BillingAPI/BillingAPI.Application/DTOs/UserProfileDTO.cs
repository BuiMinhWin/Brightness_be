using System;

namespace BillingAPI.Application.DTOs
{
    public record UserProfileDTO(
        Guid Id,
        string Email,
        string? FullName);
}

