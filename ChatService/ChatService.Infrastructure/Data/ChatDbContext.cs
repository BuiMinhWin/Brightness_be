using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.Data
{
    public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
    {
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

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
        }
    }
}
