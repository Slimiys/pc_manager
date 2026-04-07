using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Server.Application.Contracts;

namespace Server.Infrastructure.Execution;

/// <summary>
/// HTTP-клиент пересылки оповещений на Avalonia Toast Listener.
/// </summary>
public sealed class ToastListenerForwarder : IToastListenerForwarder
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ToastListenerForwarder> _logger;

    /// <summary>
    /// Создаёт клиент пересылки на toast-слушатель.
    /// </summary>
    public ToastListenerForwarder(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ToastListenerForwarder> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Notifications:ToastListenerKey"] ?? string.Empty;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ForwardAsync(string title, string? message, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/toast/show");
        request.Headers.TryAddWithoutValidation("X-Toast-Listener-Key", _apiKey);
        request.Content = JsonContent.Create(new { title, message });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Toast listener вернул {Status}: {Body}",
                (int)response.StatusCode,
                body);
        }
    }
}
