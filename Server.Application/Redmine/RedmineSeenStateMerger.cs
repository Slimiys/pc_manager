namespace Server.Application.Redmine;

/// <summary>
/// Тихое слияние выборки задач Redmine с сохранённым состоянием.
/// </summary>
public static class RedmineSeenStateMerger
{
    private const string ResolvedStatusName = "Решена";

    /// <summary>
    /// Добавляет в состояние только новые задачи из выборки, не меняя статусы уже отслеживаемых.
    /// </summary>
    /// <param name="seenState">Текущее состояние issue id → status id.</param>
    /// <param name="issues">Задачи из последнего ответа API.</param>
    public static Dictionary<string, int> MergeFetchedIssues(
        IReadOnlyDictionary<string, int> seenState,
        IReadOnlyList<RedmineIssue> issues)
    {
        var merged = new Dictionary<string, int>(seenState, StringComparer.Ordinal);

        foreach (var issue in issues)
        {
            if (ShouldSkipIssue(issue))
            {
                continue;
            }

            var issueKey = issue.Id.ToString();
            if (merged.ContainsKey(issueKey))
            {
                continue;
            }

            merged[issueKey] = issue.StatusId;
        }

        return merged;
    }

    /// <summary>
    /// Задаёт базовую линию мониторинга по текущей выборке без генерации событий.
    /// </summary>
    /// <param name="issues">Задачи из последнего ответа API.</param>
    public static Dictionary<string, int> SeedBaselineFromFetchedIssues(IReadOnlyList<RedmineIssue> issues)
    {
        var baseline = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var issue in issues)
        {
            if (ShouldSkipIssue(issue))
            {
                continue;
            }

            baseline[issue.Id.ToString()] = issue.StatusId;
        }

        return baseline;
    }

    private static bool ShouldSkipIssue(RedmineIssue issue) =>
        string.Equals(issue.StatusName, ResolvedStatusName, StringComparison.Ordinal);
}
