using BillingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingAPI.Infrastructure.Data
{
    public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
    {
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("subscription_plans");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Price).HasColumnType("numeric(10,2)");
                entity.Property(x => x.Currency).IsRequired().HasMaxLength(10);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Amount).HasColumnType("numeric(10,2)");
                entity.Property(x => x.Currency).IsRequired().HasMaxLength(10);
                entity.Property(x => x.PaymentStatus).IsRequired().HasMaxLength(50);
                entity.Property(x => x.PaymentMethod).HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });
        }
    }
}
