namespace Server.Application.Redmine;

/// <summary>
/// Результат сравнения задач Redmine с сохранённым состоянием.
/// </summary>
public sealed class RedmineMonitorResult
{
    /// <summary>
    /// Создаёт результат мониторинга.
    /// </summary>
    /// <param name="events">Обнаруженные события.</param>
    /// <param name="updatedSeenState">Обновлённое состояние issue → status id.</param>
    public RedmineMonitorResult(
        IReadOnlyList<RedmineIssueChangeEvent> events,
        IReadOnlyDictionary<string, int> updatedSeenState)
    {
        Events = events;
        UpdatedSeenState = updatedSeenState;
    }

    /// <summary>Обнаруженные события.</summary>
    public IReadOnlyList<RedmineIssueChangeEvent> Events { get; }

    /// <summary>Обновлённое состояние.</summary>
    public IReadOnlyDictionary<string, int> UpdatedSeenState { get; }
}
