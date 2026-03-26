using System.Net.Http.Json;
using Server.Application.Contracts;
using Server.Domain.Commands;

namespace Server.Infrastructure.Execution;

/// <summary>
/// Исполнитель команд через внешний host-агент.
/// </summary>
public sealed class AgentCommandExecutor : ICommandExecutor
{
    private readonly HttpClient _httpClient;
    private readonly AgentOptions _options;

    /// <summary>
    /// Создает экземпляр удаленного исполнителя команд.
    /// </summary>
    /// <param name="httpClient">HTTP клиент.</param>
    /// <param name="options">Настройки агента.</param>
    public AgentCommandExecutor(HttpClient httpClient, AgentOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<string> ExecuteAsync(CommandRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/agent/execute")
        {
            Content = JsonContent.Create(new AgentExecuteRequestDto(request.Type.ToString(), request.Payload))
        };
        httpRequest.Headers.Add("X-Agent-Key", _options.ApiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AgentExecuteResponseDto>(cancellationToken: cancellationToken);
        return payload?.Result ?? "Agent execution completed.";
    }

    private sealed record AgentExecuteRequestDto(string Type, string? Payload);

    private sealed record AgentExecuteResponseDto(string Result);
}
