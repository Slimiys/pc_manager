namespace Server.Application.Redmine;

/// <summary>
/// Тихое слияние выборки задач Redmine с сохранённым состоянием.
/// </summary>
public static class RedmineSeenStateMerger
{
    private const string ResolvedStatusName = "Решена";

    /// <summary>
    /// Обновляет статусы задач из выборки без генерации событий мониторинга.
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
            if (string.Equals(issue.StatusName, ResolvedStatusName, StringComparison.Ordinal))
            {
                continue;
            }

            merged[issue.Id.ToString()] = issue.StatusId;
        }

        return merged;
    }
}
