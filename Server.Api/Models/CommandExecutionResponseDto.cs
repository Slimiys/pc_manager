using Server.Domain.Commands;

namespace Server.Api.Models;

/// <summary>
/// DTO статуса выполнения команды.
/// </summary>
public sealed record CommandExecutionResponseDto(
    Guid CommandId,
    CommandType Type,
    CommandStatus Status,
    string? Result,
    string? Error,
    DateTimeOffset CreatedAt);
