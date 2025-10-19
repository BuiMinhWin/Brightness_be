using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationAPI.Domain.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("username")]
        public string UserName { get; set; } = null!;

        [Column("password")]
        public string PasswordHash { get; set; } = null!;

        [Column("date_of_birth", TypeName = "date")]
        public DateOnly? DateOfBirth { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("profile_picture")]
        public string? ProfilePicture { get; set; }

        [Column("role")]
        public string Role { get; set; } = null!;

        [Column("status")]
        public string Status { get; set; } = null!;

        [Column("create_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("subscription_plan_id")]
        public Guid? SubscriptionPlanId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("payment_customer_ref")]
        public string? PaymentCustomerRef { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserExternalLogin> ExternalLogins { get; set; } = new List<UserExternalLogin>();
    }
}
