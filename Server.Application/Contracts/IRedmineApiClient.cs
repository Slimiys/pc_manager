using Server.Application.Redmine;

namespace Server.Application.Contracts;

/// <summary>
/// Клиент Redmine REST API для получения задач.
/// </summary>
public interface IRedmineApiClient
{
    /// <summary>
    /// Возвращает задачи, назначенные текущему пользователю API-ключа.
    /// </summary>
    /// <param name="limit">Максимум записей.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyList<RedmineIssue>> FetchAssignedIssuesAsync(int limit, CancellationToken cancellationToken);
}
