namespace Server.Domain.Commands;

/// <summary>
/// Тип поддерживаемой команды управления компьютером.
/// </summary>
public enum CommandType
{
    LockWorkstation = 1,
    GetUptime = 2
}
