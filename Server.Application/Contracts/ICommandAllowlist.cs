using Server.Domain.Commands;

namespace Server.Application.Contracts;

/// <summary>
/// Проверяет, разрешен ли тип команды для выполнения.
/// </summary>
public interface ICommandAllowlist
{
    /// <summary>
    /// Возвращает <c>true</c>, если команда разрешена.
    /// </summary>
    /// <param name="type">Тип команды.</param>
    bool IsAllowed(CommandType type);
}
