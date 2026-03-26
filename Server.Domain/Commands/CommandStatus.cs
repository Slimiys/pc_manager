namespace Server.Domain.Commands;

/// <summary>
/// Статус выполнения команды.
/// </summary>
public enum CommandStatus
{
    Queued = 1,
    Started = 2,
    Completed = 3,
    Failed = 4
}
