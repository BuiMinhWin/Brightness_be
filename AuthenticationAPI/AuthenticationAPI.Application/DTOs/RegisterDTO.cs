using System;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Application.DTOs
{
    public record RegisterDTO
    {
        public Guid? Id { get; init; }

        [Required, EmailAddress]
        public string Email { get; init; } = default!;

        [Required]
        public string UserName { get; init; } = default!;

        [Required]
        public string Password { get; init; } = default!;

        public string? FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Gender { get; init; }
        public string? ProfilePicture { get; init; }
        public DateOnly? DateOfBirth { get; init; }

        [Required]
        public string Role { get; init; } = "user";

        [Required]
        public string Status { get; init; } = "active";

        public Guid? SubscriptionPlanId { get; init; }
        public string? PaymentCustomerRef { get; init; }
    }
}
