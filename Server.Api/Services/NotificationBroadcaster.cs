using Microsoft.AspNetCore.SignalR;
using Server.Api.Hubs;
using Server.Api.Models;
using Server.Application.Contracts;

namespace Server.Api.Services;

/// <summary>
/// Публикует оповещения в store, SignalR и toast-слушатель.
/// </summary>
public interface INotificationBroadcaster
{
    /// <summary>
    /// Рассылает оповещение подписчикам.
    /// </summary>
    /// <param name="title">Заголовок.</param>
    /// <param name="message">Текст или null.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task BroadcastAsync(string title, string? message, CancellationToken cancellationToken);
}

/// <summary>
/// Реализация рассылки оповещений.
/// </summary>
public sealed class NotificationBroadcaster : INotificationBroadcaster
{
    private readonly NotificationInMemoryStore _store;
    private readonly IHubContext<CommandEventsHub> _hubContext;
    private readonly IToastListenerForwarder _toastListenerForwarder;
    private readonly ILogger<NotificationBroadcaster> _logger;

    /// <summary>
    /// Создаёт broadcaster оповещений.
    /// </summary>
    public NotificationBroadcaster(
        NotificationInMemoryStore store,
        IHubContext<CommandEventsHub> hubContext,
        IToastListenerForwarder toastListenerForwarder,
        ILogger<NotificationBroadcaster> logger)
    {
        _store = store;
        _hubContext = hubContext;
        _toastListenerForwarder = toastListenerForwarder;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task BroadcastAsync(string title, string? message, CancellationToken cancellationToken)
    {
        var payload = new NotificationBroadcastDto(title, message, DateTimeOffset.UtcNow);
        _store.Add(payload);
        await _hubContext.Clients.All.SendAsync("NotificationReceived", payload, cancellationToken);

        try
        {
            await _toastListenerForwarder.ForwardAsync(title, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось переслать оповещение на toast-слушатель.");
        }
    }
}
