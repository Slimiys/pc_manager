using Server.Application.Contracts;
using Server.Domain.Commands;

namespace Server.Infrastructure.Allowlist;

/// <summary>
/// Базовый белый список разрешенных команд.
/// </summary>
public sealed class DefaultCommandAllowlist : ICommandAllowlist
{
    /// <inheritdoc />
    public bool IsAllowed(CommandType type) => type is CommandType.LockWorkstation or CommandType.GetUptime;
}
