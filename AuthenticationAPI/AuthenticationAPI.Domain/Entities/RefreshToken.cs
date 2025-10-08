using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationAPI.Domain.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("token")]
        public string Token { get; set; } = null!;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [Column("replaced_by_token")]
        public string? ReplacedByToken { get; set; }

        [Column("created_by_ip")]
        public string? CreatedByIp { get; set; }

        [Column("revoked_by_ip")]
        public string? RevokedByIp { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [NotMapped]
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
