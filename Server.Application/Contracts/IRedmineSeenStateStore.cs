namespace Server.Application.Contracts;

/// <summary>
/// Хранилище состояния «виденных» задач Redmine (issue id → status id).
/// </summary>
public interface IRedmineSeenStateStore
{
    /// <summary>
    /// Загружает сохранённое состояние.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Сохраняет состояние.
    /// </summary>
    Task SaveAsync(IReadOnlyDictionary<string, int> seenState, CancellationToken cancellationToken);
}
