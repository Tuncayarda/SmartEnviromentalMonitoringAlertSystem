using iotAPI.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace iotAPI.Infrastructure.Data;

public sealed class IoTDbContext : DbContext
{
    public IoTDbContext(DbContextOptions<IoTDbContext> options) : base(options)
    {
    }

    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Temperature).IsRequired();
            entity.Property(e => e.Humidity).IsRequired();
            entity.Property(e => e.AirQualityAdc).IsRequired();
            entity.Property(e => e.MotionDetected).IsRequired();
            entity.Property(e => e.Alert).IsRequired();

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.DeviceId, e.Alert });
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // PostgreSQL için DateTime'ları UTC olarak sakla
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }
}
