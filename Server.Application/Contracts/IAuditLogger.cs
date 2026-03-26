using Server.Domain.Commands;

namespace Server.Application.Contracts;

/// <summary>
/// Пишет аудит выполнения команд.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Записывает событие аудита по команде.
    /// </summary>
    /// <param name="execution">Текущее состояние команды.</param>
    Task WriteAsync(CommandExecution execution);
}
