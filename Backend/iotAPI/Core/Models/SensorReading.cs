namespace iotAPI.Core.Models;

/// <summary>
/// Sensör okuma verilerini temsil eden domain modeli
/// </summary>
public sealed class SensorReading
{
    public long Id { get; init; }

    public required string DeviceId { get; init; }

    public DateTime Timestamp { get; init; }

    public double Temperature { get; init; }

    public double Humidity { get; init; }

    public int AirQualityAdc { get; init; }

    public bool MotionDetected { get; init; }

    public bool Alert { get; init; }
}
