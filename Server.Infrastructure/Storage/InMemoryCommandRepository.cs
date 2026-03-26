using System.Collections.Concurrent;
using Server.Application.Contracts;
using Server.Domain.Commands;

namespace Server.Infrastructure.Storage;

/// <summary>
/// Потокобезопасное in-memory хранилище команд.
/// </summary>
public sealed class InMemoryCommandRepository : ICommandRepository
{
    private readonly ConcurrentDictionary<Guid, CommandExecution> storage = new();

    /// <inheritdoc />
    public Task UpsertAsync(CommandExecution execution)
    {
        storage[execution.CommandId] = execution;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<CommandExecution?> GetAsync(Guid commandId)
    {
        storage.TryGetValue(commandId, out var execution);
        return Task.FromResult(execution);
    }
}
