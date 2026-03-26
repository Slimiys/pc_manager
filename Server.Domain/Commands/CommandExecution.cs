namespace Server.Domain.Commands;

/// <summary>
/// Результат и состояние выполнения команды.
/// </summary>
public sealed class CommandExecution
{
    /// <summary>
    /// Идентификатор команды.
    /// </summary>
    public Guid CommandId { get; init; }

    /// <summary>
    /// Тип команды.
    /// </summary>
    public CommandType Type { get; init; }

    /// <summary>
    /// Автор команды.
    /// </summary>
    public string RequestedBy { get; init; } = string.Empty;

    /// <summary>
    /// Текущий статус.
    /// </summary>
    public CommandStatus Status { get; set; }

    /// <summary>
    /// Результат выполнения.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Ошибка выполнения.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Время создания команды.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
