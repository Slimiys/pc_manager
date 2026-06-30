using Server.Application.Contracts;

namespace Server.Application.Redmine;

/// <summary>
/// Тихое обновление сохранённого состояния задач Redmine после загрузки выборки.
/// </summary>
public sealed class RedmineSeenStateSync
{
    private readonly IRedmineSeenStateStore _seenStateStore;

    /// <summary>
    /// Создаёт сервис синхронизации состояния.
    /// </summary>
    /// <param name="seenStateStore">Хранилище seen issue → status id.</param>
    public RedmineSeenStateSync(IRedmineSeenStateStore seenStateStore)
    {
        _seenStateStore = seenStateStore;
    }

    /// <summary>
    /// Добавляет задачи из выборки в состояние без генерации событий мониторинга.
    /// </summary>
    /// <param name="issues">Задачи из ответа Redmine API.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task MergeAndSaveFetchedIssuesAsync(
        IReadOnlyList<RedmineIssue> issues,
        CancellationToken cancellationToken)
    {
        var seen = await _seenStateStore.LoadAsync(cancellationToken);
        var merged = RedmineSeenStateMerger.MergeFetchedIssues(seen, issues);
        var pruned = RedmineSeenStatePruner.PruneToFetchedIssues(merged, issues);
        await _seenStateStore.SaveAsync(pruned, cancellationToken);
    }
}
