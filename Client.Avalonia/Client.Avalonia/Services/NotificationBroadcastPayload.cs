namespace Client.Avalonia.Services;

/// <summary>
/// Данные входящего оповещения из SignalR (соответствует серверной модели рассылки).
/// </summary>
public sealed class NotificationBroadcastPayload
{
    /// <summary>
    /// Получает или задаёт заголовок оповещения.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Получает или задаёт дополнительный текст.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Получает или задаёт время получения на сервере (UTC).
    /// </summary>
    public DateTimeOffset ReceivedAtUtc { get; init; }
}
