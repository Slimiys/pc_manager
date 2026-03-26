using Server.Domain.Commands;

namespace Server.Application.Contracts;

/// <summary>
/// Хранилище состояний выполнения команд.
/// </summary>
public interface ICommandRepository
{
    /// <summary>
    /// Сохраняет состояние выполнения команды.
    /// </summary>
    /// <param name="execution">Состояние команды.</param>
    Task UpsertAsync(CommandExecution execution);

    /// <summary>
    /// Возвращает состояние команды по идентификатору.
    /// </summary>
    /// <param name="commandId">Идентификатор команды.</param>
    Task<CommandExecution?> GetAsync(Guid commandId);
}
