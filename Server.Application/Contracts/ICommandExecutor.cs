using Server.Domain.Commands;

namespace Server.Application.Contracts;

/// <summary>
/// Выполняет поддерживаемые команды управления.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Выполняет команду и возвращает текстовый результат.
    /// </summary>
    /// <param name="request">Запрос команды.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<string> ExecuteAsync(CommandRequest request, CancellationToken cancellationToken);
}
