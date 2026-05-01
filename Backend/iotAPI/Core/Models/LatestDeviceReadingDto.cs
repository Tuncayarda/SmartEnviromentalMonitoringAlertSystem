namespace iotAPI.Core.Models;

/// <summary>
/// Cihaz bazlı son okuma verilerini temsil eden DTO
/// </summary>
public sealed record LatestDeviceReadingDto(
    string DeviceId,
    DateTime Timestamp,
    double Temperature,
    double Humidity,
    int AirQualityAdc,
    bool MotionDetected,
    bool Alert
);
