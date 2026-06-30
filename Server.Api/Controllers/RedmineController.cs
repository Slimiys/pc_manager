using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Models;
using Server.Api.Services;
using Server.Application.Contracts;
using Server.Application.Redmine;

namespace Server.Api.Controllers;

/// <summary>
/// API статуса и событий мониторинга Redmine.
/// </summary>
[ApiController]
[Route("api/redmine")]
[Authorize(Roles = "Operator,Admin")]
public sealed class RedmineController : ControllerBase
{
    private readonly RedmineOptions _options;
    private readonly IRedmineMonitoringSwitch _monitoringSwitch;
    private readonly IRedmineIssueFetchLimit _issueFetchLimit;
    private readonly IRedmineApiClient _redmineApiClient;
    private readonly RedmineSeenStateSync _seenStateSync;
    private readonly RedmineRuntimeStatus _runtimeStatus;
    private readonly RedmineEventStore _eventStore;
    private readonly ILogger<RedmineController> _logger;

    /// <summary>
    /// Создаёт контроллер Redmine.
    /// </summary>
    public RedmineController(
        RedmineOptions options,
        IRedmineMonitoringSwitch monitoringSwitch,
        IRedmineIssueFetchLimit issueFetchLimit,
        IRedmineApiClient redmineApiClient,
        RedmineSeenStateSync seenStateSync,
        RedmineRuntimeStatus runtimeStatus,
        RedmineEventStore eventStore,
        ILogger<RedmineController> logger)
    {
        _options = options;
        _monitoringSwitch = monitoringSwitch;
        _issueFetchLimit = issueFetchLimit;
        _redmineApiClient = redmineApiClient;
        _seenStateSync = seenStateSync;
        _runtimeStatus = runtimeStatus;
        _eventStore = eventStore;
        _logger = logger;
    }

    /// <summary>
    /// Возвращает текущий статус мониторинга Redmine.
    /// </summary>
    [HttpGet("status")]
    public ActionResult<RedmineStatusDto> GetStatus() => Ok(CreateStatusDto());

    /// <summary>
    /// Включает или отключает фоновый мониторинг Redmine.
    /// </summary>
    /// <param name="request">Новое состояние.</param>
    [HttpPut("enabled")]
    public ActionResult<RedmineStatusDto> SetEnabled([FromBody] SetRedmineEnabledRequestDto request)
    {
        _monitoringSwitch.SetEnabled(request.Enabled);
        _logger.LogInformation("Redmine monitoring {State} через API.", request.Enabled ? "включён" : "выключен");
        return GetStatus();
    }

    /// <summary>
    /// Устанавливает лимит задач за один запрос опроса Redmine.
    /// </summary>
    /// <param name="request">Новый лимит.</param>
    [HttpPut("issue-fetch-limit")]
    public ActionResult<SetRedmineIssueFetchLimitResponseDto> SetIssueFetchLimit(
        [FromBody] SetRedmineIssueFetchLimitRequestDto request,
        CancellationToken cancellationToken)
    {
        _issueFetchLimit.SetValue(request.IssueFetchLimit);
        _logger.LogInformation("Redmine issue fetch limit установлен: {Limit}.", _issueFetchLimit.Value);

        return Ok(new SetRedmineIssueFetchLimitResponseDto(CreateStatusDto(), []));
    }

    /// <summary>
    /// Возвращает события Redmine после указанного времени.
    /// </summary>
    /// <param name="sinceUtc">Нижняя граница UTC.</param>
    [HttpGet("events")]
    public ActionResult<IReadOnlyList<RedmineEventDto>> GetEvents([FromQuery] DateTimeOffset sinceUtc)
    {
        return Ok(_eventStore.GetNewerThan(sinceUtc));
    }

    /// <summary>
    /// Запрашивает последние задачи из Redmine (для проверки подключения и отображения).
    /// </summary>
    /// <param name="limit">Максимум записей (1–50).</param>
    /// <param name="syncSeenState">Добавить новые задачи из выборки в состояние мониторинга.</param>
    /// <param name="seedBaseline">Задать базовую линию мониторинга по выборке (при включении).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet("issues")]
    public async Task<ActionResult<IReadOnlyList<RedmineIssueDto>>> GetLatestIssuesAsync(
        [FromQuery] int limit,
        [FromQuery] bool syncSeenState,
        [FromQuery] bool seedBaseline,
        CancellationToken cancellationToken)
    {
        var fetchLimit = Math.Clamp(limit <= 0 ? _issueFetchLimit.Value : limit, 1, 50);
        try
        {
            var issues = await _redmineApiClient.FetchAssignedIssuesAsync(fetchLimit, cancellationToken);
            if (seedBaseline)
            {
                await _seenStateSync.SeedBaselineAsync(issues, cancellationToken);
                _runtimeStatus.UpdatePollResult(true, null, issues.Count);
            }
            else if (syncSeenState && _monitoringSwitch.IsEnabled)
            {
                await _seenStateSync.MergeAndSaveFetchedIssuesAsync(issues, cancellationToken);
                _runtimeStatus.UpdatePollResult(true, null, issues.Count);
            }

            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var dtos = issues
                .Select(issue => ToIssueDto(issue, baseUrl))
                .ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить задачи Redmine для предпросмотра.");
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to fetch issues from Redmine.");
        }
    }

    private RedmineStatusDto CreateStatusDto()
    {
        var snapshot = _runtimeStatus.GetSnapshot();
        var connected = snapshot.LastSuccessfulPollUtc.HasValue
            && snapshot.LastPollAttemptUtc.HasValue
            && snapshot.LastSuccessfulPollUtc == snapshot.LastPollAttemptUtc
            && string.IsNullOrWhiteSpace(snapshot.LastError);

        return new RedmineStatusDto(
            Enabled: _monitoringSwitch.IsEnabled,
            Connected: _monitoringSwitch.IsEnabled && connected,
            NotificationsEnabled: _options.NotificationsEnabled,
            BaseUrl: string.IsNullOrWhiteSpace(_options.BaseUrl) ? null : _options.BaseUrl,
            FiltersDescription: BuildFiltersDescription(),
            LastSuccessfulPollUtc: snapshot.LastSuccessfulPollUtc,
            LastPollAttemptUtc: snapshot.LastPollAttemptUtc,
            LastError: snapshot.LastError,
            TrackedIssuesCount: snapshot.TrackedIssuesCount,
            IssueFetchLimit: _issueFetchLimit.Value);
    }

    private static RedmineIssueDto ToIssueDto(RedmineIssue issue, string baseUrl) =>
        new(
            issue.Id,
            issue.Subject,
            issue.StatusName,
            issue.PriorityName,
            issue.VersionName,
            issue.CreatedOnUtc,
            issue.UpdatedOnUtc,
            $"{baseUrl}/issues/{issue.Id}");

    private string BuildFiltersDescription()
    {
        var parts = new List<string> { "assigned_to_id=me" };
        if (!string.IsNullOrWhiteSpace(_options.ProjectId))
        {
            parts.Add($"project_id={_options.ProjectId}");
        }

        if (!string.IsNullOrWhiteSpace(_options.FixedVersionId))
        {
            parts.Add($"fixed_version_id={_options.FixedVersionId}");
        }

        return string.Join(", ", parts);
    }
}
