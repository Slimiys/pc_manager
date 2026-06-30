namespace Server.Api.Models;

/// <summary>
/// Статус фонового мониторинга Redmine.
/// </summary>
public sealed record RedmineStatusDto(
    bool Enabled,
    bool Connected,
    bool NotificationsEnabled,
    string? BaseUrl,
    string? FiltersDescription,
    DateTimeOffset? LastSuccessfulPollUtc,
    DateTimeOffset? LastPollAttemptUtc,
    string? LastError,
    int TrackedIssuesCount,
    int IssueFetchLimit);
