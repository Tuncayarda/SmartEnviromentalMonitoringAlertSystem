namespace iotAPI.Core.Interfaces;

/// <summary>
/// MQTT mesajlarını işleyen servis arayüzü
/// </summary>
public interface IMqttMessageHandler
{
    Task HandleMessageAsync(string payload, CancellationToken cancellationToken = default);
}
