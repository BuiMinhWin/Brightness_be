using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.Data
{
    public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
    {
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ConversationSummary> ConversationSummaries => Set<ConversationSummary>();
        public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();
        
        // Shared tables from other services (read-only access)
        public DbSet<User> Users => Set<User>();
        public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("chat");

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.ToTable("chat_sessions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                // Context caching fields
                entity.Property(x => x.UserContextSnapshot).HasColumnType("jsonb");
                entity.Property(x => x.ContextUpdatedAt);
                
                // Index for cache invalidation queries
                entity.HasIndex(x => new { x.UserId, x.ContextUpdatedAt });
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

            modelBuilder.Entity<AiUsageLog>(entity =>
            {
                entity.ToTable("ai_usage_logs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Operation).HasMaxLength(100);
                entity.Property(x => x.Model).HasMaxLength(100);
                entity.Property(x => x.UserName).HasMaxLength(200);
                entity.Property(x => x.PromptTokens).HasDefaultValue(0);
                entity.Property(x => x.CompletionTokens).HasDefaultValue(0);
                entity.Property(x => x.TotalTokens).HasDefaultValue(0);
                entity.Property(x => x.EstimatedCost).HasColumnType("numeric(18,6)").HasDefaultValue(0);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.SessionId);
            });

            // Configure shared entities (read-only, no migrations)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users", "authentication");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            modelBuilder.Entity<BodyMeasurement>(entity =>
            {
                entity.ToTable("body_measurements", "measurement");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("subscription_plans", "billing");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("user_profiles", "personalcoach");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });

            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.ToTable("user_preferences", "personalcoach");
                entity.HasKey(x => x.Id);
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });
        }
    }
}
