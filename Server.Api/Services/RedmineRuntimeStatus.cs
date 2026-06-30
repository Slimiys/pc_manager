namespace Server.Api.Services;

/// <summary>
/// Текущее runtime-состояние опроса Redmine для API статуса.
/// </summary>
public sealed class RedmineRuntimeStatus
{
    private readonly object _sync = new();

    /// <summary>
    /// Обновляет состояние после попытки опроса.
    /// </summary>
    /// <param name="connected">Успешен ли опрос.</param>
    /// <param name="error">Текст ошибки или null.</param>
    /// <param name="trackedIssuesCount">Число отслеживаемых задач.</param>
    public void UpdatePollResult(bool connected, string? error, int trackedIssuesCount)
    {
        lock (_sync)
        {
            LastPollAttemptUtc = DateTimeOffset.UtcNow;
            LastError = error;
            TrackedIssuesCount = trackedIssuesCount;
            if (connected)
            {
                LastSuccessfulPollUtc = LastPollAttemptUtc;
            }
        }
    }

    /// <summary>
    /// Возвращает снимок состояния.
    /// </summary>
    public Snapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new Snapshot(
                LastSuccessfulPollUtc,
                LastPollAttemptUtc,
                LastError,
                TrackedIssuesCount);
        }
    }

    /// <summary>Снимок runtime-состояния.</summary>
    public sealed record Snapshot(
        DateTimeOffset? LastSuccessfulPollUtc,
        DateTimeOffset? LastPollAttemptUtc,
        string? LastError,
        int TrackedIssuesCount);

    private DateTimeOffset? LastSuccessfulPollUtc { get; set; }

    private DateTimeOffset? LastPollAttemptUtc { get; set; }

    private string? LastError { get; set; }

    private int TrackedIssuesCount { get; set; }
}
