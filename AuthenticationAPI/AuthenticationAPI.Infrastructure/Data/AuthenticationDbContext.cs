using AuthenticationAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Infrastructure.Data
{
    public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
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
        }
    }
}
