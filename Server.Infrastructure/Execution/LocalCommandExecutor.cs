using Server.Application.Contracts;
using Server.Domain.Commands;

namespace Server.Infrastructure.Execution;

/// <summary>
/// Локальный исполнитель типизированных команд.
/// </summary>
public sealed class LocalCommandExecutor : ICommandExecutor
{
    private readonly IWorkstationLocker _workstationLocker;

    /// <summary>
    /// Создает экземпляр исполнителя команд.
    /// </summary>
    /// <param name="workstationLocker">Сервис блокировки рабочей станции.</param>
    public LocalCommandExecutor(IWorkstationLocker workstationLocker)
    {
        _workstationLocker = workstationLocker;
    }

    /// <inheritdoc />
    public Task<string> ExecuteAsync(CommandRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return request.Type switch
        {
            CommandType.GetUptime => Task.FromResult(
                (DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64)).ToString()),
            CommandType.LockWorkstation => Task.FromResult(LockWorkstation()),
            _ => throw new InvalidOperationException($"Unsupported command type: {request.Type}")
        };
    }

    private string LockWorkstation()
    {
        _workstationLocker.Lock();
        return "Workstation lock requested successfully.";
    }
}
