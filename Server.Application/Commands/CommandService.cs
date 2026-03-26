using Server.Application.Contracts;
using Server.Domain.Commands;
using System.Text.Json;

namespace Server.Application.Commands;

/// <summary>
/// Реализация orchestration слоя выполнения команд.
/// </summary>
public sealed class CommandService : ICommandService
{
    private readonly ICommandAllowlist _commandAllowlist;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ICommandRepository _commandRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Создает сервис команд.
    /// </summary>
    public CommandService(
        ICommandAllowlist commandAllowlist,
        ICommandExecutor commandExecutor,
        ICommandRepository commandRepository,
        IAuditLogger auditLogger,
        IEventPublisher eventPublisher)
    {
        _commandAllowlist = commandAllowlist;
        _commandExecutor = commandExecutor;
        _commandRepository = commandRepository;
        _auditLogger = auditLogger;
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc />
    public async Task<CommandExecution> ExecuteAsync(CommandRequest request, CancellationToken cancellationToken)
    {
        var existing = await _commandRepository.GetAsync(request.CommandId);
        if (existing is not null)
        {
            return existing;
        }

        if (!_commandAllowlist.IsAllowed(request.Type))
        {
            throw new InvalidOperationException($"Command '{request.Type}' is not allowed.");
        }

        var execution = new CommandExecution
        {
            CommandId = request.CommandId,
            Type = request.Type,
            RequestedBy = request.RequestedBy,
            Status = CommandStatus.Queued
        };
        await _commandRepository.UpsertAsync(execution);
        await _auditLogger.WriteAsync(execution);
        await PublishAuditAsync(execution, cancellationToken);

        execution.Status = CommandStatus.Started;
        await _commandRepository.UpsertAsync(execution);
        await _auditLogger.WriteAsync(execution);
        await PublishAuditAsync(execution, cancellationToken);

        try
        {
            execution.Result = await _commandExecutor.ExecuteAsync(request, cancellationToken);
            execution.Status = CommandStatus.Completed;
        }
        catch (Exception ex)
        {
            execution.Status = CommandStatus.Failed;
            execution.Error = ex.Message;
        }

        await _commandRepository.UpsertAsync(execution);
        await _auditLogger.WriteAsync(execution);
        await PublishAuditAsync(execution, cancellationToken);
        return execution;
    }

    /// <inheritdoc />
    public Task<CommandExecution?> GetByIdAsync(Guid commandId) => _commandRepository.GetAsync(commandId);

    private Task PublishAuditAsync(CommandExecution execution, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            execution.CommandId,
            execution.Type,
            execution.Status,
            execution.RequestedBy,
            execution.CreatedAt,
            execution.Result,
            execution.Error
        });
        return _eventPublisher.PublishAsync("pc.commands.audit", payload, cancellationToken);
    }
}
