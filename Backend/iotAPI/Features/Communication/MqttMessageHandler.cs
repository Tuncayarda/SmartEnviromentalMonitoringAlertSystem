using System.Text.Json;
using iotAPI.Core.Interfaces;
using iotAPI.Core.Models;
using Microsoft.Extensions.Logging;

namespace iotAPI.Features.Communication;

public sealed class MqttMessageHandler : IMqttMessageHandler
{
    private readonly ISensorDataRepository _repository;
    private readonly ILogger<MqttMessageHandler> _logger;

    public MqttMessageHandler(
        ISensorDataRepository repository,
        ILogger<MqttMessageHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleMessageAsync(string payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        _logger.LogInformation("[MQTT RECEIVED] Raw Data: {Payload}", payload);

        try
        {
            var sensorData = ParseSensorData(payload);
            var reading = MapToSensorReading(sensorData);

            await _repository.AddAsync(reading, cancellationToken);

            _logger.LogInformation(
                "[DB SAVED] Device: {DeviceId} | Time: {Timestamp:yyyy-MM-dd HH:mm:ss} | Temp: {Temperature:F1}C | Humidity: {Humidity:F1}% | AirADC: {AirQuality} | Motion: {Motion} | Alert: {Alert}",
                reading.DeviceId,
                reading.Timestamp,
                reading.Temperature,
                reading.Humidity,
                reading.AirQualityAdc,
                reading.MotionDetected,
                reading.Alert);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[PARSE ERROR] Failed to parse MQTT message: {Payload}", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PROCESSING ERROR] Failed to process sensor data");
        }
    }

    private static SensorDataDto ParseSensorData(string payload)
    {
        var jsonDoc = JsonDocument.Parse(payload);
        var root = jsonDoc.RootElement;

        return new SensorDataDto(
            DeviceId: root.GetProperty("device_id").GetString() ?? throw new JsonException("device_id bulunamadı"),
            Timestamp: root.GetProperty("timestamp").GetString() ?? throw new JsonException("timestamp bulunamadı"),
            Temperature: root.GetProperty("temperature").GetDouble(),
            Humidity: root.GetProperty("humidity").GetDouble(),
            AirAdc: root.GetProperty("air_adc").GetInt32(),
            Motion: root.GetProperty("motion").GetBoolean(),
            Alert: root.GetProperty("alert").GetBoolean()
        );
    }

    private static SensorReading MapToSensorReading(SensorDataDto dto)
    {
        var timestampOffset = DateTimeOffset.Parse(dto.Timestamp);
        var timestamp = timestampOffset.UtcDateTime;

        return new SensorReading
        {
            DeviceId = dto.DeviceId,
            Timestamp = timestamp,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            AirQualityAdc = dto.AirAdc,
            MotionDetected = dto.Motion,
            Alert = dto.Alert
        };
    }
}
