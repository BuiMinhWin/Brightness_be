using MeasurementAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeasurementAPI.Infrastructure.Data
{
    public class MeasurementDbContext(DbContextOptions<MeasurementDbContext> options) : DbContext(options)
    {
        public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<BodyMeasurement>(entity =>
            {
                entity.ToTable("body_measurements");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.HeightCm).HasColumnType("numeric(5,2)");
                entity.Property(x => x.WeightKg).HasColumnType("numeric(5,2)");
                entity.Property(x => x.BodyFatPercent).HasColumnType("numeric(5,2)");
                entity.Property(x => x.WaistCm).HasColumnType("numeric(5,2)");
                entity.Property(x => x.ChestCm).HasColumnType("numeric(5,2)");
                entity.Property(x => x.HipCm).HasColumnType("numeric(5,2)");
                entity.Property(x => x.ActivityLevel).HasMaxLength(128);
                entity.Property(x => x.Goal).HasMaxLength(256);
                entity.Property(x => x.HealthNote).HasMaxLength(1024);
                entity.Property(x => x.RecordedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });
        }
    }
}
