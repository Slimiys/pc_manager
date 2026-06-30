using Server.Api.Models;
using Server.Application.Contracts;
using Server.Application.Redmine;

namespace Server.Api.Services;

/// <summary>
/// Один цикл фонового опроса Redmine.
/// </summary>
public sealed class RedminePollRunner : IRedminePollRunner
{
    private readonly IRedmineIssueFetchLimit _issueFetchLimit;
    private readonly RedmineOptions _options;
    private readonly IRedmineApiClient _redmineApiClient;
    private readonly IRedmineSeenStateStore _seenStateStore;
    private readonly RedmineIssueMonitor _issueMonitor;
    private readonly INotificationBroadcaster _notificationBroadcaster;
    private readonly RedmineEventStore _eventStore;
    private readonly RedmineRuntimeStatus _runtimeStatus;
    private readonly ILogger<RedminePollRunner> _logger;
    private readonly SemaphoreSlim _pollGate = new(1, 1);

    /// <summary>
    /// Создаёт исполнитель цикла опроса Redmine.
    /// </summary>
    public RedminePollRunner(
        IRedmineIssueFetchLimit issueFetchLimit,
        RedmineOptions options,
        IRedmineApiClient redmineApiClient,
        IRedmineSeenStateStore seenStateStore,
        RedmineIssueMonitor issueMonitor,
        INotificationBroadcaster notificationBroadcaster,
        RedmineEventStore eventStore,
        RedmineRuntimeStatus runtimeStatus,
        ILogger<RedminePollRunner> logger)
    {
        _issueFetchLimit = issueFetchLimit;
        _options = options;
        _redmineApiClient = redmineApiClient;
        _seenStateStore = seenStateStore;
        _issueMonitor = issueMonitor;
        _notificationBroadcaster = notificationBroadcaster;
        _eventStore = eventStore;
        _runtimeStatus = runtimeStatus;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<RedminePollResult> RunAsync(CancellationToken cancellationToken) =>
        RunAsync(RedminePollOptions.Monitoring, cancellationToken);

    /// <inheritdoc />
    public async Task<RedminePollResult> RunAsync(RedminePollOptions options, CancellationToken cancellationToken)
    {
        await _pollGate.WaitAsync(cancellationToken);
        try
        {
            return await PollCoreAsync(options, cancellationToken);
        }
        finally
        {
            _pollGate.Release();
        }
    }

    private async Task<RedminePollResult> PollCoreAsync(RedminePollOptions options, CancellationToken cancellationToken)
    {
        var seen = await _seenStateStore.LoadAsync(cancellationToken);
        var issues = await _redmineApiClient.FetchAssignedIssuesAsync(
            _issueFetchLimit.Value,
            cancellationToken);

        Dictionary<string, int> prunedState;
        IReadOnlyList<RedmineIssueChangeEvent> changeEvents;

        if (options.DetectChanges)
        {
            var result = _issueMonitor.DetectChanges(issues, seen, _options.BaseUrl);
            prunedState = RedmineSeenStatePruner.PruneToFetchedIssues(result.UpdatedSeenState, issues);
            changeEvents = result.Events;
        }
        else
        {
            var merged = RedmineSeenStateMerger.MergeFetchedIssues(seen, issues);
            prunedState = RedmineSeenStatePruner.PruneToFetchedIssues(merged, issues);
            changeEvents = [];
        }

        await _seenStateStore.SaveAsync(prunedState, cancellationToken);
        _runtimeStatus.UpdatePollResult(true, null, issues.Count);

        foreach (var changeEvent in changeEvents)
        {
            var dto = ToDto(changeEvent);
            _eventStore.Add(dto);

            if (_options.NotificationsEnabled)
            {
                await _notificationBroadcaster.BroadcastAsync(
                    changeEvent.Title,
                    changeEvent.Message,
                    cancellationToken);
            }
        }

        _logger.LogDebug(
            "Redmine poll ({Mode}): лимит {Limit}, задач {Count}, событий {Events}.",
            options.DetectChanges ? "monitoring" : "limit-sync",
            _issueFetchLimit.Value,
            issues.Count,
            changeEvents.Count);

        return new RedminePollResult(issues, changeEvents.Count);
    }

    private static RedmineEventDto ToDto(RedmineIssueChangeEvent changeEvent) =>
        new(
            changeEvent.IssueId,
            changeEvent.Kind.ToString(),
            changeEvent.Title,
            changeEvent.Message,
            changeEvent.Subject,
            changeEvent.StatusName,
            changeEvent.StatusWithEmoji,
            changeEvent.ProjectName,
            changeEvent.VersionName,
            changeEvent.IssueUrl,
            changeEvent.CreatedOnUtc,
            changeEvent.UpdatedOnUtc,
            DateTimeOffset.UtcNow);
}
