namespace Server.Api.Models;

/// <summary>
/// Тело входящего HTTP-запроса с оповещением для рассылки подключённым клиентам.
/// </summary>
public sealed class InboundNotificationRequestDto
{
    /// <summary>
    /// Краткий заголовок оповещения.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Дополнительный текст (необязательно).
    /// </summary>
    public string? Message { get; init; }
}
