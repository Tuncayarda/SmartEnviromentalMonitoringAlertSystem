using iotAPI.Core.Interfaces;
using iotAPI.Core.Models;
using iotAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iotAPI.Infrastructure.Repositories;

public sealed class SensorDataRepository : ISensorDataRepository
{
    private readonly IoTDbContext _context;

    public SensorDataRepository(IoTDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SensorReading> AddAsync(SensorReading reading, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reading);

        await _context.SensorReadings.AddAsync(reading, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return reading;
    }

    public async Task<IReadOnlyList<LatestDeviceReadingDto>> GetLatestReadingsByDeviceAsync(
        CancellationToken cancellationToken = default)
    {
        var deviceIds = await _context.SensorReadings
            .Select(r => r.DeviceId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var latestReadings = new List<LatestDeviceReadingDto>();

        foreach (var deviceId in deviceIds)
        {
            var latestReading = await _context.SensorReadings
                .Where(r => r.DeviceId == deviceId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestReading != null)
            {
                latestReadings.Add(new LatestDeviceReadingDto(
                    latestReading.DeviceId,
                    latestReading.Timestamp,
                    latestReading.Temperature,
                    latestReading.Humidity,
                    latestReading.AirQualityAdc,
                    latestReading.MotionDetected,
                    latestReading.Alert
                ));
            }
        }

        return latestReadings;
    }

    public async Task<IReadOnlyList<SensorReading>> GetRecentReadingsByDeviceAsync(
        string deviceId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        return await _context.SensorReadings
            .Where(r => r.DeviceId == deviceId)
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SensorReading>> GetAllAlertsAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.SensorReadings
            .Where(r => r.Alert)
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SensorReading>> GetAlertsByDeviceIdAsync(
        string deviceId, 
        int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var alerts = await _context.SensorReadings
            .Where(r => r.DeviceId == deviceId && r.Alert)
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return alerts;
    }
}
