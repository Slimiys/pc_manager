using Server.Domain.Commands;

namespace Server.Application.Commands;

/// <summary>
/// Фасад для запуска команд и чтения их статуса.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Выполняет команду и возвращает актуальный статус.
    /// </summary>
    /// <param name="request">Запрос на выполнение команды.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<CommandExecution> ExecuteAsync(CommandRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает текущее состояние команды.
    /// </summary>
    /// <param name="commandId">Идентификатор команды.</param>
    Task<CommandExecution?> GetByIdAsync(Guid commandId);
}
