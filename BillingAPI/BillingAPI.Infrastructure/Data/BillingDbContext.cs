using BillingAPI.Domain.Entities;
using BillingAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingAPI.Infrastructure.Data
{
    public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
    {
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("billing");

            ConfigureSubscriptionPlan(modelBuilder.Entity<SubscriptionPlan>());
            ConfigureInvoice(modelBuilder.Entity<Invoice>());
            ConfigurePayment(modelBuilder.Entity<Payment>());
        }

        private static void ConfigureSubscriptionPlan(EntityTypeBuilder<SubscriptionPlan> entity)
        {
            entity.ToTable("subscription_plans");
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Name).IsRequired().HasMaxLength(200);
            entity.Property(plan => plan.Description).HasMaxLength(1024);
            entity.Property(plan => plan.Price).HasColumnType("numeric(12,2)");
            entity.Property(plan => plan.Currency).IsRequired().HasMaxLength(10);
            entity.Property(plan => plan.DurationDays).HasDefaultValue(30);
            entity.Property(plan => plan.MonthlyAICredits).HasDefaultValue(0);
            entity.Property(plan => plan.IsActive).HasDefaultValue(true);
            entity.Property(plan => plan.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.HasIndex(plan => plan.Name).IsUnique();
        }

        private static void ConfigureInvoice(EntityTypeBuilder<Invoice> entity)
        {
            entity.ToTable("invoices");
            entity.HasKey(invoice => invoice.Id);
            entity.Property(invoice => invoice.InvoiceCode).IsRequired().HasMaxLength(64);
            entity.HasIndex(invoice => invoice.InvoiceCode).IsUnique();
            entity.Property(invoice => invoice.Amount).HasColumnType("numeric(12,2)");
            entity.Property(invoice => invoice.Currency).IsRequired().HasMaxLength(10);
            entity.Property(invoice => invoice.Status).IsRequired().HasMaxLength(32);
            entity.Property(invoice => invoice.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(invoice => invoice.DueDate).HasDefaultValueSql("now() at time zone 'utc' + interval '3 days'");
            entity.Property(invoice => invoice.Notes).HasMaxLength(2048);

            entity.HasOne(invoice => invoice.SubscriptionPlan)
                .WithMany(plan => plan.Invoices)
                .HasForeignKey(invoice => invoice.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigurePayment(EntityTypeBuilder<Payment> entity)
        {
            entity.ToTable("payments");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.Amount).HasColumnType("numeric(12,2)");
            entity.Property(payment => payment.Currency).IsRequired().HasMaxLength(10);
            entity.Property(payment => payment.PaymentStatus).IsRequired().HasMaxLength(32);
            entity.Property(payment => payment.TransactionId).HasMaxLength(128);
            entity.Property(payment => payment.CheckoutUrl).HasMaxLength(1024);
            entity.Property(payment => payment.RawGatewayPayload);
            entity.Property(payment => payment.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(payment => payment.Gateway)
                .HasConversion(
                    gateway => gateway.ToString(),
                    value => ParseGateway(value))
                .HasMaxLength(32);

            entity.HasOne(payment => payment.Invoice)
                .WithMany(invoice => invoice.Payments)
                .HasForeignKey(payment => payment.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static PaymentGatewayType ParseGateway(string value)
        {
            return Enum.TryParse<PaymentGatewayType>(value, out var parsed) ? parsed : PaymentGatewayType.PayOs;
        }
    }
}
