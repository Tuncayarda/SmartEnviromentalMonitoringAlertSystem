using iotAPI.Core.Models;

namespace iotAPI.Core.Interfaces;

/// <summary>
/// Sensör verisi veri erişim katmanı arayüzü
/// </summary>
public interface ISensorDataRepository
{
    Task<SensorReading> AddAsync(SensorReading reading, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LatestDeviceReadingDto>> GetLatestReadingsByDeviceAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SensorReading>> GetRecentReadingsByDeviceAsync(string deviceId, int limit = 10, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SensorReading>> GetAlertsByDeviceIdAsync(string deviceId, int limit = 10, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SensorReading>> GetAllAlertsAsync(int limit = 20, CancellationToken cancellationToken = default);
}
