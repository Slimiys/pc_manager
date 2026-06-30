using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Client.Avalonia.Services;

/// <summary>
/// Клиент API мониторинга Redmine на сервере.
/// </summary>
public sealed class RedmineApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Создаёт клиент Redmine API.
    /// </summary>
    /// <param name="httpClient">HTTP-клиент с BaseAddress Server.Api.</param>
    public RedmineApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Возвращает статус мониторинга Redmine.
    /// </summary>
    public async Task<RedmineStatusPayload?> GetStatusAsync(string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/redmine/status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RedmineStatusPayload>(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Возвращает события Redmine после указанного времени.
    /// </summary>
    public async Task<IReadOnlyList<RedmineEventPayload>> GetEventsSinceAsync(
        DateTimeOffset sinceUtc,
        string token,
        CancellationToken cancellationToken)
    {
        var since = Uri.EscapeDataString(sinceUtc.ToUniversalTime().ToString("o"));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/redmine/events?sinceUtc={since}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<RedmineEventPayload>>(cancellationToken: cancellationToken);
        return items ?? [];
    }

    /// <summary>
    /// Устанавливает состояние мониторинга Redmine на сервере.
    /// </summary>
    public async Task<RedmineStatusPayload?> SetEnabledAsync(
        bool enabled,
        string token,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "api/redmine/enabled");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new SetRedmineEnabledRequestDto(enabled));
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RedmineStatusPayload>(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Устанавливает лимит задач Redmine за один запрос на сервере.
    /// </summary>
    public async Task<SetIssueFetchLimitResult?> SetIssueFetchLimitAsync(
        int limit,
        string token,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "api/redmine/issue-fetch-limit");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new SetRedmineIssueFetchLimitRequestDto(limit));
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content
            .ReadFromJsonAsync<SetIssueFetchLimitResponsePayload>(cancellationToken: cancellationToken);
        if (payload?.Status is null)
        {
            return null;
        }

        return new SetIssueFetchLimitResult
        {
            Status = payload.Status,
            Issues = payload.Issues ?? []
        };
    }

    /// <summary>
    /// Запрашивает последние задачи из Redmine через сервер.
    /// </summary>
    /// <param name="limit">Максимум записей; 0 — лимит с сервера.</param>
    /// <param name="syncSeenState">Синхронизировать состояние мониторинга на сервере.</param>
    public async Task<IReadOnlyList<RedmineIssuePayload>> GetLatestIssuesAsync(
        string token,
        CancellationToken cancellationToken,
        int limit = 0,
        bool syncSeenState = false)
    {
        var query = new List<string>();
        if (limit > 0)
        {
            query.Add($"limit={limit}");
        }

        if (syncSeenState)
        {
            query.Add("syncSeenState=true");
        }

        var queryString = query.Count > 0 ? $"?{string.Join('&', query)}" : string.Empty;
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/redmine/issues{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<RedmineIssuePayload>>(cancellationToken: cancellationToken);
        return items ?? [];
    }
}

/// <summary>
/// Тело запроса включения мониторинга Redmine.
/// </summary>
internal sealed record SetRedmineEnabledRequestDto(bool Enabled);

/// <summary>
/// Тело запроса изменения лимита задач Redmine.
/// </summary>
internal sealed record SetRedmineIssueFetchLimitRequestDto(int IssueFetchLimit);

/// <summary>
/// Ответ сервера на изменение лимита задач Redmine.
/// </summary>
internal sealed class SetIssueFetchLimitResponsePayload
{
    /// <summary>Статус мониторинга.</summary>
    public RedmineStatusPayload? Status { get; init; }

    /// <summary>Задачи из внепланового опроса.</summary>
    public List<RedmineIssuePayload>? Issues { get; init; }
}

/// <summary>
/// Результат изменения лимита задач Redmine.
/// </summary>
public sealed class SetIssueFetchLimitResult
{
    /// <summary>Статус мониторинга.</summary>
    public required RedmineStatusPayload Status { get; init; }

    /// <summary>Задачи, загруженные сервером при опросе.</summary>
    public IReadOnlyList<RedmineIssuePayload> Issues { get; init; } = [];
}

/// <summary>
/// Статус мониторинга Redmine с сервера.
/// </summary>
public sealed class RedmineStatusPayload
{
    /// <summary>Включён ли опрос на сервере.</summary>
    public bool Enabled { get; init; }

    /// <summary>Успешен ли последний опрос.</summary>
    public bool Connected { get; init; }

    /// <summary>Включены ли оповещения.</summary>
    public bool NotificationsEnabled { get; init; }

    /// <summary>URL Redmine.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Описание фильтров.</summary>
    public string? FiltersDescription { get; init; }

    /// <summary>Время последнего успешного опроса UTC.</summary>
    public DateTimeOffset? LastSuccessfulPollUtc { get; init; }

    /// <summary>Время последней попытки опроса UTC.</summary>
    public DateTimeOffset? LastPollAttemptUtc { get; init; }

    /// <summary>Текст последней ошибки.</summary>
    public string? LastError { get; init; }

    /// <summary>Число отслеживаемых задач.</summary>
    public int TrackedIssuesCount { get; init; }

    /// <summary>Лимит задач за один запрос опроса.</summary>
    public int IssueFetchLimit { get; init; }
}

/// <summary>
/// Событие Redmine с сервера.
/// </summary>
public sealed class RedmineEventPayload
{
    /// <summary>Идентификатор задачи.</summary>
    public int IssueId { get; init; }

    /// <summary>Тип события.</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>Заголовок.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Текст.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Тема задачи.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Название статуса.</summary>
    public string StatusName { get; init; } = string.Empty;

    /// <summary>Название проекта.</summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Название версии или null.</summary>
    public string? VersionName { get; init; }

    /// <summary>URL задачи.</summary>
    public string IssueUrl { get; init; } = string.Empty;

    /// <summary>Дата создания задачи UTC.</summary>
    public DateTimeOffset CreatedOnUtc { get; init; }

    /// <summary>Дата изменения задачи UTC.</summary>
    public DateTimeOffset UpdatedOnUtc { get; init; }

    /// <summary>Время получения UTC.</summary>
    public DateTimeOffset ReceivedAtUtc { get; init; }
}

/// <summary>
/// Задача Redmine с сервера (предпросмотр).
/// </summary>
public sealed class RedmineIssuePayload
{
    /// <summary>Идентификатор задачи.</summary>
    public int Id { get; init; }

    /// <summary>Тема задачи.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Название статуса.</summary>
    public string StatusName { get; init; } = string.Empty;

    /// <summary>Название приоритета.</summary>
    public string PriorityName { get; init; } = string.Empty;

    /// <summary>Название версии или null.</summary>
    public string? VersionName { get; init; }

    /// <summary>Дата создания UTC.</summary>
    public DateTimeOffset CreatedOnUtc { get; init; }

    /// <summary>Дата изменения UTC.</summary>
    public DateTimeOffset UpdatedOnUtc { get; init; }

    /// <summary>URL задачи.</summary>
    public string IssueUrl { get; init; } = string.Empty;
}
