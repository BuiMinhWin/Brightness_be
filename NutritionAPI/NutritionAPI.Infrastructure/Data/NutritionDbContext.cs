using Microsoft.EntityFrameworkCore;
using NutritionAPI.Domain.Entities;

namespace NutritionAPI.Infrastructure.Data
{
    public class NutritionDbContext(DbContextOptions<NutritionDbContext> options) : DbContext(options)
    {
        public DbSet<NutritionPlan> NutritionPlans => Set<NutritionPlan>();
        public DbSet<MealLog> MealLogs => Set<MealLog>();
        public DbSet<MealFood> MealFoods => Set<MealFood>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("nutrition");

            modelBuilder.Entity<NutritionPlan>(entity =>
            {
                entity.ToTable("nutrition_plans");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CalorieTarget).HasColumnType("numeric(6,2)");
                entity.Property(x => x.ProteinGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.CarbGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.FatGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<MealLog>(entity =>
            {
                entity.ToTable("meal_logs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.LoggedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.Property(x => x.TotalCalories).HasColumnType("numeric(6,2)");
                entity.Property(x => x.ProteinGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.CarbGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.FatGrams).HasColumnType("numeric(6,2)");
            });

            modelBuilder.Entity<MealFood>(entity =>
            {
                entity.ToTable("meal_foods");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FoodName).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Calories).HasColumnType("numeric(6,2)");
                entity.Property(x => x.ProteinGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.CarbGrams).HasColumnType("numeric(6,2)");
                entity.Property(x => x.FatGrams).HasColumnType("numeric(6,2)");
            });
        }
    }
}
