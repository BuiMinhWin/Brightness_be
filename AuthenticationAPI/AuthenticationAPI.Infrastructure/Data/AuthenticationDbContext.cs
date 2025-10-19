using AuthenticationAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Infrastructure.Data
{
    public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("authentication");

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.UserName).IsUnique();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).IsRequired();
                entity.Property(u => u.Status).IsRequired();
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Token).IsUnique();
                entity.Property(t => t.Token).IsRequired();
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.Property(t => t.ExpiresAt).IsRequired();

                entity.HasOne(t => t.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserExternalLogin>(entity =>
            {
                entity.ToTable("user_external_logins");
                entity.HasKey(l => l.Id);
                entity.HasIndex(l => new { l.Provider, l.ProviderKey }).IsUnique();
                entity.Property(l => l.Provider).IsRequired();
                entity.Property(l => l.ProviderKey).IsRequired();
                entity.Property(l => l.Email).IsRequired();
                entity.Property(l => l.CreatedAt)
                    .HasDefaultValueSql("now() at time zone 'utc'");

                entity.HasOne(l => l.User)
                    .WithMany(u => u.ExternalLogins)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

