using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationAPI.Domain.Entities
{
    [Table("user_external_logins")]
    public class UserExternalLogin
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("provider")]
        public string Provider { get; set; } = null!;

        [Column("provider_key")]
        public string ProviderKey { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
