using Server.Application.Redmine;

namespace Server.Application.Redmine;

/// <summary>
/// Оставляет в состоянии только задачи из текущей выборки опроса.
/// </summary>
public static class RedmineSeenStatePruner
{
    /// <summary>
    /// Фильтрует сохранённое состояние по идентификаторам задач из ответа API.
    /// </summary>
    /// <param name="seenState">Полное состояние после сравнения.</param>
    /// <param name="issues">Задачи из последнего запроса.</param>
    public static Dictionary<string, int> PruneToFetchedIssues(
        IReadOnlyDictionary<string, int> seenState,
        IReadOnlyList<RedmineIssue> issues)
    {
        var allowed = issues
            .Select(issue => issue.Id.ToString())
            .ToHashSet(StringComparer.Ordinal);

        return seenState
            .Where(pair => allowed.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }
}
