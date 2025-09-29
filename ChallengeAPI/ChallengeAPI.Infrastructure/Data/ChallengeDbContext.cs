using ChallengeAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Infrastructure.Data
{
    public class ChallengeDbContext(DbContextOptions<ChallengeDbContext> options) : DbContext(options)
    {
        public DbSet<Challenge> Challenges => Set<Challenge>();
        public DbSet<ChallengeParticipant> ChallengeParticipants => Set<ChallengeParticipant>();
        public DbSet<ChallengeRule> ChallengeRules => Set<ChallengeRule>();
        public DbSet<ChallengeTask> ChallengeTasks => Set<ChallengeTask>();
        public DbSet<ChallengeTaskProgress> ChallengeTaskProgresses => Set<ChallengeTaskProgress>();
        public DbSet<Achievement> Achievements => Set<Achievement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Challenge>(entity =>
            {
                entity.ToTable("challenges");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<ChallengeParticipant>(entity =>
            {
                entity.ToTable("challenge_participants");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.JoinedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.Property(x => x.TotalPoints).HasColumnType("numeric(10,2)");
                entity.Property(x => x.PercentageCompleted).HasColumnType("numeric(5,2)");
            });

            modelBuilder.Entity<ChallengeRule>(entity =>
            {
                entity.ToTable("challenge_rules");
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<ChallengeTask>(entity =>
            {
                entity.ToTable("challenge_tasks");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Points).HasColumnType("numeric(8,2)");
                entity.Property(x => x.StartDate).HasColumnType("date");
                entity.Property(x => x.EndDate).HasColumnType("date");
            });

            modelBuilder.Entity<ChallengeTaskProgress>(entity =>
            {
                entity.ToTable("challenge_task_progress");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TargetDate).HasColumnType("date");
                entity.Property(x => x.PointsEarned).HasColumnType("numeric(8,2)");
                entity.Property(x => x.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.ToTable("achievements");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Code).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });
        }
    }
}
