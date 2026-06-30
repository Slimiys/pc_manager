namespace Server.Application.Redmine;

/// <summary>
/// Сравнивает задачи Redmine с сохранённым состоянием и формирует события.
/// </summary>
public sealed class RedmineIssueMonitor
{
    private const string ResolvedStatusName = "Решена";

    private static readonly IReadOnlyDictionary<string, string> StatusEmojiMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Новая"] = "⚪",
            ["В работе"] = "🟢",
            ["Обратная связь"] = "💬",
            ["Решена"] = "✅",
            ["Закрыта"] = "🔒",
            ["Отменена"] = "🚫",
            ["Блокирована"] = "⛔",
            ["Открыта"] = "📌",
            ["Тестирование"] = "🧪",
            ["Ожидание"] = "⏳"
        };

    /// <summary>
    /// Обнаруживает новые задачи и смену статуса по аналогии с redmine_notify.py.
    /// </summary>
    /// <param name="issues">Задачи из последнего ответа API.</param>
    /// <param name="seenState">Сохранённое состояние issue id → status id.</param>
    /// <param name="baseUrl">Базовый URL Redmine.</param>
    public RedmineMonitorResult DetectChanges(
        IReadOnlyList<RedmineIssue> issues,
        IReadOnlyDictionary<string, int> seenState,
        string baseUrl)
    {
        var updated = new Dictionary<string, int>(seenState, StringComparer.Ordinal);
        var events = new List<RedmineIssueChangeEvent>();
        var normalizedBaseUrl = baseUrl.TrimEnd('/');

        foreach (var issue in issues)
        {
            var issueKey = issue.Id.ToString();
            var isResolved = string.Equals(issue.StatusName, ResolvedStatusName, StringComparison.Ordinal);

            if (!updated.TryGetValue(issueKey, out var previousStatusId))
            {
                if (isResolved)
                {
                    continue;
                }

                events.Add(CreateEvent(issue, RedmineIssueChangeKind.NewIssue, normalizedBaseUrl));
                updated[issueKey] = issue.StatusId;
                continue;
            }

            if (previousStatusId != issue.StatusId)
            {
                events.Add(CreateEvent(issue, RedmineIssueChangeKind.StatusChanged, normalizedBaseUrl));
                updated[issueKey] = issue.StatusId;
            }
        }

        return new RedmineMonitorResult(events, updated);
    }

    private static RedmineIssueChangeEvent CreateEvent(
        RedmineIssue issue,
        RedmineIssueChangeKind kind,
        string baseUrl)
    {
        var statusWithEmoji = FormatStatusWithEmoji(issue.StatusName);
        var prefix = BuildPrefix(issue.ProjectName, issue.VersionName);
        var issueUrl = $"{baseUrl}/issues/{issue.Id}";
        var description = issue.Description ?? string.Empty;
        var descriptionSnippet = description.Length > 100 ? description[..100] : description;

        var title = kind == RedmineIssueChangeKind.StatusChanged
            ? $"{prefix} Redmine: {issue.Subject} (Статус изменён)"
            : $"{prefix} Redmine: {issue.Subject}";

        var message = kind == RedmineIssueChangeKind.StatusChanged
            ? $"Статус: {statusWithEmoji} | #{issue.Id} {issue.Subject}"
            : $"Статус: {statusWithEmoji} | #{issue.Id} {descriptionSnippet}";

        return new RedmineIssueChangeEvent
        {
            IssueId = issue.Id,
            Kind = kind,
            Title = title,
            Message = message,
            Subject = issue.Subject,
            StatusName = issue.StatusName,
            StatusWithEmoji = statusWithEmoji,
            ProjectName = issue.ProjectName,
            VersionName = issue.VersionName,
            IssueUrl = issueUrl,
            CreatedOnUtc = issue.CreatedOnUtc,
            UpdatedOnUtc = issue.UpdatedOnUtc
        };
    }

    private static string BuildPrefix(string projectName, string? versionName)
    {
        if (!string.IsNullOrWhiteSpace(versionName))
        {
            return $"[{versionName}][{projectName}]";
        }

        return $"[{projectName}]";
    }

    private static string FormatStatusWithEmoji(string statusName)
    {
        if (StatusEmojiMap.TryGetValue(statusName, out var emoji))
        {
            return $"{emoji} {statusName}";
        }

        return $"📍 {statusName}";
    }
}
