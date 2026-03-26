using Server.Application.Contracts;
using Server.Domain.Commands;

namespace Server.Infrastructure.Audit;

/// <summary>
/// Простой аудит в консоль без чувствительных данных.
/// </summary>
public sealed class ConsoleAuditLogger : IAuditLogger
{
    /// <inheritdoc />
    public Task WriteAsync(CommandExecution execution)
    {
        Console.WriteLine(
            "AUDIT CommandId={0}; Type={1}; Status={2}; RequestedBy={3}; CreatedAt={4:O}",
            execution.CommandId,
            execution.Type,
            execution.Status,
            execution.RequestedBy,
            execution.CreatedAt);
        return Task.CompletedTask;
    }
}
