using iotAPI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace iotAPI.Features.Communication;

public sealed class MqttListenerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttConfiguration _config;
    private readonly ILogger<MqttListenerService> _logger;
    private IMqttClient? _mqttClient;
    private MqttClientOptions? _mqttOptions;
    private CancellationTokenSource? _reconnectCts;

    public MqttListenerService(
        IServiceScopeFactory scopeFactory,
        IOptions<MqttConfiguration> config,
        ILogger<MqttListenerService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[MQTT SERVICE] Starting...");

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_config.Host, _config.Port)
            .WithClientId(_config.ClientId)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))
            .WithCleanSession()
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _mqttClient.ConnectedAsync += OnConnectedAsync;
        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

        await TryConnectAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[MQTT SERVICE] Stopping...");
        }
    }

    private async Task TryConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_mqttClient == null || _mqttOptions == null)
                return;

            _logger.LogInformation("[MQTT] Connecting to {Host}:{Port}...", _config.Host, _config.Port);
            await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MQTT] Connection failed");
        }
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        _reconnectCts?.Cancel();
        _reconnectCts?.Dispose();
        _reconnectCts = null;

        _logger.LogInformation("[MQTT] Connected to broker!");

        try
        {
            await _mqttClient!.SubscribeAsync(_config.Topic);

            _logger.LogInformation(
                "[MQTT] Subscribed to topic '{Topic}' successfully",
                _config.Topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT] Failed to subscribe to topic");
        }
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("[MQTT] Disconnected. Reason: {Reason} - Starting reconnection...", args.Reason);

        _reconnectCts?.Cancel();
        _reconnectCts?.Dispose();
        _reconnectCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            var attemptCount = 0;
            while (!_reconnectCts.Token.IsCancellationRequested)
            {
                try
                {
                    attemptCount++;

                    if (!_mqttClient?.IsConnected ?? false)
                    {
                        _logger.LogInformation("[MQTT] Reconnection attempt #{Attempt}...", attemptCount);
                        await TryConnectAsync(_reconnectCts.Token);

                        if (_mqttClient?.IsConnected ?? false)
                        {
                            _logger.LogInformation("[MQTT] Reconnection successful!");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    await Task.Delay(1000, _reconnectCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[MQTT] Reconnection attempt #{Attempt} failed", attemptCount);
                }
            }
        }, _reconnectCts.Token);

        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var messageHandler = scope.ServiceProvider.GetRequiredService<IMqttMessageHandler>();

            var payload = args.ApplicationMessage.ConvertPayloadToString();
            await messageHandler.HandleMessageAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT] Error processing message");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _reconnectCts?.Cancel();
        _reconnectCts?.Dispose();

        if (_mqttClient is not null && _mqttClient.IsConnected)
        {
            _logger.LogInformation("[MQTT] Disconnecting gracefully...");
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            _mqttClient.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}

public sealed class MqttConfiguration
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1883;
    public string ClientId { get; init; } = "iotAPI";
    public string Topic { get; init; } = "sensors";
}
