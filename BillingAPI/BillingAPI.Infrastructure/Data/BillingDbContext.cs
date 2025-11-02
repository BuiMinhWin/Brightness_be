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
        public DbSet<AICreditPool> AICreditPools => Set<AICreditPool>();
        public DbSet<AIOperationLog> AIOperationLogs => Set<AIOperationLog>();
        public DbSet<CreditCostConfiguration> CreditCostConfigurations => Set<CreditCostConfiguration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("billing");

            ConfigureSubscriptionPlan(modelBuilder.Entity<SubscriptionPlan>());
            ConfigureInvoice(modelBuilder.Entity<Invoice>());
            ConfigurePayment(modelBuilder.Entity<Payment>());
            ConfigureAICreditPool(modelBuilder.Entity<AICreditPool>());
            ConfigureAIOperationLog(modelBuilder.Entity<AIOperationLog>());
            ConfigureCreditCostConfiguration(modelBuilder.Entity<CreditCostConfiguration>());
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
            entity.Property(plan => plan.CreditTier).IsRequired().HasMaxLength(50).HasDefaultValue("free");
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

        private static void ConfigureAICreditPool(EntityTypeBuilder<AICreditPool> entity)
        {
            entity.ToTable("ai_credit_pools");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.UserId).IsRequired();
            entity.Property(p => p.ServiceName).IsRequired().HasMaxLength(50);
            entity.Property(p => p.SubscriptionTier).IsRequired().HasMaxLength(50);
            entity.Property(p => p.MonthlyTokenLimit).IsRequired();
            entity.Property(p => p.RemainingTokens).IsRequired();
            entity.Property(p => p.CycleStartDate).IsRequired();
            entity.Property(p => p.CycleEndDate).IsRequired();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.HasIndex(p => new { p.UserId, p.ServiceName }).IsUnique();
        }

        private static void ConfigureAIOperationLog(EntityTypeBuilder<AIOperationLog> entity)
        {
            entity.ToTable("ai_operation_logs");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.UserId).IsRequired();
            entity.Property(l => l.CreditPoolId).IsRequired();
            entity.Property(l => l.ServiceName).IsRequired().HasMaxLength(50);
            entity.Property(l => l.OperationType).IsRequired().HasMaxLength(100);
            entity.Property(l => l.Model).IsRequired().HasMaxLength(100);
            entity.Property(l => l.PromptTokens).IsRequired();
            entity.Property(l => l.CompletionTokens).IsRequired();
            entity.Property(l => l.TotalTokens).IsRequired();
            entity.Property(l => l.TokenCost).IsRequired();
            entity.Property(l => l.EstimatedMonetaryCost).HasColumnType("numeric(12,6)");
            entity.Property(l => l.RelatedEntityId).HasMaxLength(100);
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            
            entity.HasOne(l => l.CreditPool)
                .WithMany(p => p.OperationLogs)
                .HasForeignKey(l => l.CreditPoolId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.UserId);
            entity.HasIndex(l => l.CreditPoolId);
            entity.HasIndex(l => l.CreatedAt);
        }

        private static void ConfigureCreditCostConfiguration(EntityTypeBuilder<CreditCostConfiguration> entity)
        {
            entity.ToTable("credit_cost_configurations");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ServiceName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.OperationType).IsRequired().HasMaxLength(100);
            entity.Property(c => c.BaseTokenMultiplier).HasColumnType("numeric(10,2)");
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.HasIndex(c => new { c.ServiceName, c.OperationType }).IsUnique();
        }
    }
}
