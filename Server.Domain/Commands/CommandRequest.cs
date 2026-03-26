namespace Server.Domain.Commands;

/// <summary>
/// Запрос на выполнение команды.
/// </summary>
/// <param name="CommandId">Идентификатор команды для идемпотентности.</param>
/// <param name="Type">Тип команды.</param>
/// <param name="Payload">Дополнительные параметры команды.</param>
/// <param name="RequestedBy">Идентификатор пользователя.</param>
public sealed record CommandRequest(Guid CommandId, CommandType Type, string? Payload, string RequestedBy);
