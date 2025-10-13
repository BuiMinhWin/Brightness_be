using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.Data
{
    public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
    {
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ConversationSummary> ConversationSummaries => Set<ConversationSummary>();
        
        // Shared tables from other services (read-only access)
        public DbSet<User> Users => Set<User>();
        public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.ToTable("chat_sessions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_messages");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Role).IsRequired().HasMaxLength(20);
                entity.Property(x => x.Content).IsRequired();
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.Session)
                    .WithMany()
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ConversationSummary>(entity =>
            {
                entity.ToTable("conversation_summaries");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.SummaryText).IsRequired();
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.Session)
                    .WithMany()
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure shared entities (read-only, no migrations)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            modelBuilder.Entity<BodyMeasurement>(entity =>
            {
                entity.ToTable("body_measurements");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("subscription_plans");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });
        }
    }
}
