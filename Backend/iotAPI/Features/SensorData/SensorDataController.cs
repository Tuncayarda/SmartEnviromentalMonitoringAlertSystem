using iotAPI.Core.Interfaces;
using iotAPI.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace iotAPI.Features.SensorData;

/// <summary>
/// Sensör verilerini ve alarmları sorgulayan API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SensorDataController : ControllerBase
{
    private readonly ISensorDataRepository _repository;
    private readonly ILogger<SensorDataController> _logger;

    public SensorDataController(
        ISensorDataRepository repository,
        ILogger<SensorDataController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Tüm cihazların en son okunan verilerini getirir
    /// </summary>
    /// <returns>Cihaz bazlı son sensör verileri</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IReadOnlyList<LatestDeviceReadingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestReadings(CancellationToken cancellationToken)
    {
        try
        {
            var readings = await _repository.GetLatestReadingsByDeviceAsync(cancellationToken);
            return Ok(readings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Son veriler getirilirken hata oluştu");
            return StatusCode(StatusCodes.Status500InternalServerError, "Veriler getirilirken bir hata oluştu");
        }
    }

    [HttpGet("recent/{deviceId}")]
    [ProducesResponseType(typeof(IReadOnlyList<SensorReading>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentByDevice(
        [FromRoute] string deviceId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID boş olamaz");

        try
        {
            var readings = await _repository.GetRecentReadingsByDeviceAsync(deviceId, limit, cancellationToken);
            return Ok(readings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Son kayıtlar getirilirken hata oluştu. DeviceId: {DeviceId}", deviceId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Veriler getirilirken bir hata oluştu");
        }
    }

    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IReadOnlyList<SensorReading>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAlerts(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _repository.GetAllAlertsAsync(limit, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alert'ler getirilirken hata oluştu");
            return StatusCode(StatusCodes.Status500InternalServerError, "Alert verileri getirilirken bir hata oluştu");
        }
    }

    [HttpGet("alerts/{deviceId}")]
    [ProducesResponseType(typeof(IReadOnlyList<SensorReading>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAlertsByDevice(
        [FromRoute] string deviceId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("Device ID boş olamaz");

        try
        {
            var alerts = await _repository.GetAlertsByDeviceIdAsync(deviceId, limit, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cihaz alert'leri getirilirken hata oluştu. DeviceId: {DeviceId}", deviceId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Alert verileri getirilirken bir hata oluştu");
        }
    }
}
