using MeasurementAPI.Domain.Entities;
using MeasurementAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeasurementAPI.Infrastructure.Data
{
    public class MeasurementDbContext(DbContextOptions<MeasurementDbContext> options) : DbContext(options)
    {
        public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
        public DbSet<MeasurementDevice> MeasurementDevices => Set<MeasurementDevice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureBodyMeasurement(modelBuilder.Entity<BodyMeasurement>());
            ConfigureMeasurementDevice(modelBuilder.Entity<MeasurementDevice>());
        }

        private static void ConfigureBodyMeasurement(EntityTypeBuilder<BodyMeasurement> entity)
        {
            entity.ToTable("body_measurements");
            entity.HasKey(measurement => measurement.Id);
            entity.Property(measurement => measurement.HeightCm).HasColumnType("numeric(6,2)");
            entity.Property(measurement => measurement.WeightKg).HasColumnType("numeric(6,2)");
            entity.Property(measurement => measurement.BodyFatPercent).HasColumnType("numeric(5,2)");
            entity.Property(measurement => measurement.SkeletalMuscleMassKg).HasColumnType("numeric(6,2)");
            entity.Property(measurement => measurement.TotalBodyWaterPercent).HasColumnType("numeric(5,2)");
            entity.Property(measurement => measurement.BasalMetabolicRate).HasColumnType("numeric(8,2)");
            entity.Property(measurement => measurement.WaistCm).HasColumnType("numeric(6,2)");
            entity.Property(measurement => measurement.ChestCm).HasColumnType("numeric(6,2)");
            entity.Property(measurement => measurement.HipCm).HasColumnType("numeric(6,2)");
            entity.Property(measurement => measurement.ActivityLevel).HasMaxLength(128);
            entity.Property(measurement => measurement.Goal).HasMaxLength(256);
            entity.Property(measurement => measurement.HealthNote).HasMaxLength(1024);
            entity.Property(measurement => measurement.RawData).HasColumnType("jsonb");
            entity.Property(measurement => measurement.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(measurement => measurement.RecordedAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(measurement => measurement.Source)
                .HasConversion(
                    source => source.ToString(),
                    value => ParseSource(value))
                .HasMaxLength(32);

            entity.HasOne(measurement => measurement.Device)
                .WithMany(device => device.Measurements)
                .HasForeignKey(measurement => measurement.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(measurement => new { measurement.UserId, measurement.RecordedAt });
        }

        private static void ConfigureMeasurementDevice(EntityTypeBuilder<MeasurementDevice> entity)
        {
            entity.ToTable("measurement_devices");
            entity.HasKey(device => device.Id);
            entity.Property(device => device.DeviceType).IsRequired().HasMaxLength(64);
            entity.Property(device => device.Manufacturer).HasMaxLength(128);
            entity.Property(device => device.Model).HasMaxLength(128);
            entity.Property(device => device.SerialNumber).HasMaxLength(128);
            entity.Property(device => device.FirmwareVersion).HasMaxLength(64);
            entity.Property(device => device.RegisteredAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(device => device.IsActive).HasDefaultValue(true);
            entity.Property(device => device.LastSyncedAt);
            entity.Property(device => device.LastSyncStartedAt);
            entity.Property(device => device.LastSyncCompletedAt);
            entity.Property(device => device.LastSyncSuccess);
            entity.Property(device => device.LastSyncMessage).HasMaxLength(512);
            entity.Property(device => device.LastSyncPayload).HasColumnType("text");

            entity.HasIndex(device => new { device.UserId, device.DeviceType });
            entity.HasIndex(device => new { device.UserId, device.SerialNumber }).IsUnique().HasFilter("\"SerialNumber\" IS NOT NULL");
        }

        private static MeasurementSourceType ParseSource(string value)
        {
            return Enum.TryParse<MeasurementSourceType>(value, out var parsed) ? parsed : MeasurementSourceType.Manual;
        }
    }
}
