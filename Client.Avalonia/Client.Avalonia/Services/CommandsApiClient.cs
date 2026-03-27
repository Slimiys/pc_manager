using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Avalonia.Services;

/// <summary>
/// Клиент командного API.
/// </summary>
public sealed class CommandsApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Создает клиент командного API.
    /// </summary>
    /// <param name="httpClient">HTTP клиент.</param>
    public CommandsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Выполняет команду и возвращает результат.
    /// </summary>
    /// <param name="token">JWT токен.</param>
    /// <param name="commandType">Тип команды.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<string> ExecuteCommandAsync(string token, string commandType, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/commands/execute");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ExecuteRequestDto(commandType, null));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<ExecuteResponseDto>(cancellationToken: cancellationToken);
        if (data is null)
        {
            throw new InvalidOperationException("API returned empty execute response.");
        }

        if (!string.IsNullOrWhiteSpace(data.Error))
        {
            throw new InvalidOperationException(data.Error);
        }

        var statusText = data.Status?.ToString() ?? "unknown";
        return string.IsNullOrWhiteSpace(data.Result)
            ? $"Status: {statusText}"
            : data.Result;
    }

    /// <summary>
    /// Выполняет команду GetUptime.
    /// </summary>
    /// <param name="token">JWT токен.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public Task<string> ExecuteGetUptimeAsync(string token, CancellationToken cancellationToken)
    {
        return ExecuteCommandAsync(token, "GetUptime", cancellationToken);
    }

    /// <summary>
    /// Выполняет команду LockWorkstation.
    /// </summary>
    /// <param name="token">JWT токен.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public Task<string> ExecuteLockWorkstationAsync(string token, CancellationToken cancellationToken)
    {
        return ExecuteCommandAsync(token, "LockWorkstation", cancellationToken);
    }

    private sealed record ExecuteRequestDto(string Type, string? Payload);

    private sealed record ExecuteResponseDto(JsonElement? Status, string? Result, string? Error);
}
