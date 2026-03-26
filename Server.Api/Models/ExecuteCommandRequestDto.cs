namespace Server.Api.Models;

/// <summary>
/// DTO запроса на выполнение команды.
/// </summary>
public sealed class ExecuteCommandRequestDto
{
    /// <summary>
    /// Тип команды.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Дополнительные параметры команды.
    /// </summary>
    public string? Payload { get; init; }
}
