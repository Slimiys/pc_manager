using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Client.Avalonia.Services;

/// <summary>
/// Клиент API оповещений (опрос новых записей).
/// </summary>
public sealed class NotificationsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;

    /// <summary>
    /// Создаёт клиент оповещений.
    /// </summary>
    /// <param name="httpClient">HTTP-клиент с базовым адресом API.</param>
    /// <param name="apiSettings">Настройки API-клиента.</param>
    public NotificationsApiClient(HttpClient httpClient, ApiSettings apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings;
    }

    /// <summary>
    /// Возвращает оповещения сервера с временем строго после указанной отметки (UTC).
    /// </summary>
    /// <param name="sinceUtc">Нижняя граница по времени.</param>
    /// <param name="token">JWT.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<IReadOnlyList<NotificationBroadcastPayload>> GetRecentSinceAsync(
        DateTimeOffset sinceUtc,
        string token,
        CancellationToken cancellationToken)
    {
        var encoded = Uri.EscapeDataString(sinceUtc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/notifications/recent?sinceUtc={encoded}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var list = await response.Content
            .ReadFromJsonAsync<List<NotificationBroadcastPayload>>(cancellationToken: cancellationToken);
        return list ?? [];
    }

    /// <summary>
    /// Отправляет входящее оповещение на сервер для последующей пересылки агенту.
    /// </summary>
    /// <param name="title">Заголовок оповещения.</param>
    /// <param name="message">Текст оповещения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SendInboundAsync(string title, string message, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/notifications/inbound");
        request.Headers.TryAddWithoutValidation("X-Notification-Key", _apiSettings.InboundNotificationKey);
        request.Content = JsonContent.Create(new InboundNotificationRequestDto(title, message));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed record InboundNotificationRequestDto(string Title, string Message);
}
