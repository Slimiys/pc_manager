namespace Server.Api.Models;

/// <summary>
/// Полезная нагрузка события SignalR при рассылке оповещения.
/// </summary>
/// <param name="Title">Краткий заголовок.</param>
/// <param name="Message">Дополнительный текст или null.</param>
/// <param name="ReceivedAtUtc">Момент приёма на сервере (UTC).</param>
public sealed record NotificationBroadcastDto(string Title, string? Message, DateTimeOffset ReceivedAtUtc);
