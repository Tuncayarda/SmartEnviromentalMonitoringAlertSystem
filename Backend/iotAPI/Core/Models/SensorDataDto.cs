namespace iotAPI.Core.Models;

/// <summary>
/// MQTT'den gelen ham sensör verisini temsil eden DTO
/// </summary>
public sealed record SensorDataDto(
    string DeviceId,
    string Timestamp,
    double Temperature,
    double Humidity,
    int AirAdc,
    bool Motion,
    bool Alert
);
