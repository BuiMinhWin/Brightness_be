using Microsoft.EntityFrameworkCore;
using WorkoutAPI.Domain.Entities;

namespace WorkoutAPI.Infrastructure.Data
{
    public class WorkoutDbContext(DbContextOptions<WorkoutDbContext> options) : DbContext(options)
    {
        public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
        public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
        public DbSet<SessionExercise> SessionExercises => Set<SessionExercise>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("workout");

            modelBuilder.Entity<WorkoutPlan>(entity =>
            {
                entity.ToTable("workout_plans");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<WorkoutSession>(entity =>
            {
                entity.ToTable("workout_sessions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.SessionDate).HasColumnType("date");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasOne<WorkoutPlan>()
                    .WithMany(p => p.Sessions)
                    .HasForeignKey(x => x.WorkoutPlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SessionExercise>(entity =>
            {
                entity.ToTable("session_exercises");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ExerciseName).IsRequired().HasMaxLength(200);
                entity.Property(x => x.WeightKg).HasColumnType("numeric(6,2)");
                entity.HasOne<WorkoutSession>()
                    .WithMany(s => s.Exercises)
                    .HasForeignKey(x => x.WorkoutSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
