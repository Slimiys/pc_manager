using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Server.Api.Models;
using Server.Api.Services;

namespace Server.Api.Controllers;

/// <summary>
/// Приём внешних оповещений по HTTP, хранение и рассылка подписчикам SignalR.
/// </summary>
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private const int MaxTitleLength = 200;
    private const int MaxMessageLength = 4000;

    private readonly IConfiguration _configuration;
    private readonly INotificationBroadcaster _notificationBroadcaster;
    private readonly NotificationInMemoryStore _store;
    private readonly ILogger<NotificationsController> _logger;

    /// <summary>
    /// Создаёт контроллер оповещений.
    /// </summary>
    public NotificationsController(
        IConfiguration configuration,
        INotificationBroadcaster notificationBroadcaster,
        NotificationInMemoryStore store,
        ILogger<NotificationsController> logger)
    {
        _configuration = configuration;
        _notificationBroadcaster = notificationBroadcaster;
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Возвращает оповещения, полученные сервером после указанного времени (для опроса UI).
    /// </summary>
    /// <param name="sinceUtc">Нижняя граница по времени в UTC (формат round-trip «O»).</param>
    [HttpGet("recent")]
    [Authorize(Roles = "Operator,Admin")]
    public ActionResult<IReadOnlyList<NotificationBroadcastDto>> GetRecent([FromQuery] DateTimeOffset sinceUtc)
    {
        var items = _store.GetNewerThan(sinceUtc);
        return Ok(items);
    }

    /// <summary>
    /// Принимает оповещение от внешней системы (webhook) и рассылает его через SignalR.
    /// </summary>
    /// <param name="request">Текст оповещения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("inbound")]
    [AllowAnonymous]
    [DisableRateLimiting]
    public async Task<IActionResult> InboundAsync(
        [FromBody] InboundNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        var expectedKey = (_configuration["Notifications:InboundApiKey"] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (!Request.Headers.TryGetValue("X-Notification-Key", out var rawKey))
        {
            _logger.LogWarning("Inbound: отсутствует заголовок X-Notification-Key.");
            return Unauthorized();
        }

        var provided = rawKey.ToString().Trim();
        if (!string.Equals(provided, expectedKey, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Inbound: неверный X-Notification-Key (длина получена {ProvidedLen}, ожидается {ExpectedLen}).",
                provided.Length,
                expectedKey.Length);
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var title = request.Title.Trim();
        if (title.Length > MaxTitleLength)
        {
            return BadRequest($"Title exceeds {MaxTitleLength} characters.");
        }

        string? message = null;
        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            message = request.Message.Trim();
            if (message.Length > MaxMessageLength)
            {
                return BadRequest($"Message exceeds {MaxMessageLength} characters.");
            }
        }

        await _notificationBroadcaster.BroadcastAsync(title, message, cancellationToken);

        return Ok();
    }
}
