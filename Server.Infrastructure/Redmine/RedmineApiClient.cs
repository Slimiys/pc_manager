using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Server.Application.Contracts;
using Server.Application.Redmine;

namespace Server.Infrastructure.Redmine;

/// <summary>
/// HTTP-клиент Redmine REST API.
/// </summary>
public sealed class RedmineApiClient : IRedmineApiClient
{
    private readonly HttpClient _httpClient;
    private readonly RedmineOptions _options;
    private readonly SemaphoreSlim _priorityCacheGate = new(1, 1);
    private IReadOnlyDictionary<int, string>? _priorityNamesCache;

    /// <summary>
    /// Создаёт клиент Redmine.
    /// </summary>
    /// <param name="httpClient">HTTP-клиент.</param>
    /// <param name="options">Настройки Redmine.</param>
    public RedmineApiClient(HttpClient httpClient, RedmineOptions options)
    {
        _httpClient = httpClient;
        _options = options;
        _httpClient.Timeout = TimeSpan.FromSeconds(
            Math.Max(1, _options.ConnectTimeoutSeconds + _options.ReadTimeoutSeconds));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RedmineIssue>> FetchAssignedIssuesAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Redmine:ApiKey не задан.");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("Redmine:BaseUrl не задан.");
        }

        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var query = new List<string>
        {
            $"limit={limit}",
            "assigned_to_id=me",
            "sort=created_on:desc"
        };

        if (!string.IsNullOrWhiteSpace(_options.ProjectId))
        {
            query.Add($"project_id={Uri.EscapeDataString(_options.ProjectId)}");
        }

        if (!string.IsNullOrWhiteSpace(_options.FixedVersionId))
        {
            query.Add($"fixed_version_id={Uri.EscapeDataString(_options.FixedVersionId)}");
        }

        var requestUri = $"{baseUrl}/issues.json?{string.Join('&', query)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.TryAddWithoutValidation("X-Redmine-API-Key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var hint = GetHttpErrorHint((int)response.StatusCode);
            var hintSuffix = string.IsNullOrWhiteSpace(hint) ? string.Empty : $" {hint}";
            throw new HttpRequestException($"Redmine HTTP {(int)response.StatusCode}.{hintSuffix}");
        }

        var payload = await response.Content.ReadFromJsonAsync<RedmineIssuesResponseDto>(cancellationToken);
        if (payload?.Issues is null)
        {
            return Array.Empty<RedmineIssue>();
        }

        var priorityNames = await FetchPriorityNamesAsync(baseUrl, cancellationToken);

        return payload.Issues
            .Select(issue => MapIssue(issue, priorityNames))
            .ToList();
    }

    private async Task<IReadOnlyDictionary<int, string>> FetchPriorityNamesAsync(
        string baseUrl,
        CancellationToken cancellationToken)
    {
        if (_priorityNamesCache is not null)
        {
            return _priorityNamesCache;
        }

        await _priorityCacheGate.WaitAsync(cancellationToken);
        try
        {
            if (_priorityNamesCache is not null)
            {
                return _priorityNamesCache;
            }

            var requestUri = $"{baseUrl}/issue_priorities.json";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.TryAddWithoutValidation("X-Redmine-API-Key", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _priorityNamesCache = new Dictionary<int, string>();
                return _priorityNamesCache;
            }

            var payload = await response.Content.ReadFromJsonAsync<RedminePrioritiesResponseDto>(cancellationToken);
            if (payload?.IssuePriorities is null)
            {
                _priorityNamesCache = new Dictionary<int, string>();
                return _priorityNamesCache;
            }

            _priorityNamesCache = payload.IssuePriorities
                .Where(p => p.Id > 0 && !string.IsNullOrWhiteSpace(p.Name))
                .ToDictionary(p => p.Id, p => p.Name!);

            return _priorityNamesCache;
        }
        finally
        {
            _priorityCacheGate.Release();
        }
    }

    private static RedmineIssue MapIssue(
        RedmineIssueJsonDto dto,
        IReadOnlyDictionary<int, string> priorityNames)
    {
        var priorityId = dto.Priority?.Id ?? dto.PriorityId;
        var priorityName = dto.Priority?.Name;
        if (string.IsNullOrWhiteSpace(priorityName)
            && priorityId > 0
            && priorityNames.TryGetValue(priorityId, out var resolvedName))
        {
            priorityName = resolvedName;
        }

        return new RedmineIssue
        {
            Id = dto.Id,
            Subject = dto.Subject ?? string.Empty,
            Description = dto.Description,
            StatusId = dto.Status?.Id ?? 0,
            StatusName = dto.Status?.Name ?? string.Empty,
            PriorityName = priorityName ?? string.Empty,
            ProjectName = dto.Project?.Name ?? string.Empty,
            VersionName = dto.FixedVersion?.Name,
            CreatedOnUtc = dto.CreatedOn.ToUniversalTime(),
            UpdatedOnUtc = dto.UpdatedOn.ToUniversalTime()
        };
    }

    private static string GetHttpErrorHint(int statusCode) =>
        statusCode switch
        {
            401 => "Неверный или отозванный API-ключ.",
            403 => "Доступ запрещён. Проверьте права пользователя и ProjectId.",
            404 => "Ресурс не найден. Проверьте BaseUrl и ProjectId.",
            _ => string.Empty
        };

    private sealed class RedmineIssuesResponseDto
    {
        [JsonPropertyName("issues")]
        public List<RedmineIssueJsonDto>? Issues { get; init; }
    }

    private sealed class RedmineIssueJsonDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("subject")]
        public string? Subject { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("status")]
        public RedmineNamedIdDto? Status { get; init; }

        [JsonPropertyName("priority")]
        public RedmineNamedIdDto? Priority { get; init; }

        [JsonPropertyName("priority_id")]
        public int PriorityId { get; init; }

        [JsonPropertyName("project")]
        public RedmineNamedIdDto? Project { get; init; }

        [JsonPropertyName("fixed_version")]
        public RedmineNamedIdDto? FixedVersion { get; init; }

        [JsonPropertyName("created_on")]
        public DateTimeOffset CreatedOn { get; init; }

        [JsonPropertyName("updated_on")]
        public DateTimeOffset UpdatedOn { get; init; }
    }

    private sealed class RedminePrioritiesResponseDto
    {
        [JsonPropertyName("issue_priorities")]
        public List<RedmineNamedIdDto>? IssuePriorities { get; init; }
    }

    private sealed class RedmineNamedIdDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
