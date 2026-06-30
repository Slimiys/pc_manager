namespace Server.Application.Contracts;

using Server.Application.Redmine;

/// <summary>
/// Выполняет один цикл опроса Redmine и публикации событий.
/// </summary>
public interface IRedminePollRunner
{
    /// <summary>
    /// Выполняет полный цикл мониторинга Redmine.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат опроса с загруженными задачами.</returns>
    Task<RedminePollResult> RunAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Загружает задачи, сравнивает с состоянием и рассылает события.
    /// </summary>
    /// <param name="options">Режим опроса.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат опроса с загруженными задачами.</returns>
    Task<RedminePollResult> RunAsync(RedminePollOptions options, CancellationToken cancellationToken);
}
