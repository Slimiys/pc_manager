using Server.Application.Commands;
using Server.Application.Contracts;
using Server.Domain.Commands;

namespace Server.Application.Tests;

public sealed class CommandServiceTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCompleteAllowedCommand()
    {
        var repository = new FakeRepository();
        var service = new CommandService(
            new AllowAllAllowlist(),
            new SuccessExecutor(),
            repository,
            new FakeAuditLogger(),
            new FakeEventPublisher());

        var request = new CommandRequest(Guid.NewGuid(), CommandType.GetUptime, null, "tester");

        var result = await service.ExecuteAsync(request, CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(CommandStatus.Completed));
        Assert.That(result.Result, Is.EqualTo("ok"));
        Assert.That((await repository.GetAsync(request.CommandId))?.Status, Is.EqualTo(CommandStatus.Completed));
    }

    [Test]
    public void ExecuteAsync_ShouldRejectNotAllowedCommand()
    {
        var service = new CommandService(
            new DenyAllAllowlist(),
            new SuccessExecutor(),
            new FakeRepository(),
            new FakeAuditLogger(),
            new FakeEventPublisher());

        var request = new CommandRequest(Guid.NewGuid(), CommandType.LockWorkstation, null, "tester");

        Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(request, CancellationToken.None));
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnExistingCommandForSameId()
    {
        var commandId = Guid.NewGuid();
        var repository = new FakeRepository();
        await repository.UpsertAsync(new CommandExecution
        {
            CommandId = commandId,
            Type = CommandType.GetUptime,
            RequestedBy = "tester",
            Status = CommandStatus.Completed,
            Result = "cached"
        });
        var service = new CommandService(
            new AllowAllAllowlist(),
            new SuccessExecutor(),
            repository,
            new FakeAuditLogger(),
            new FakeEventPublisher());

        var request = new CommandRequest(commandId, CommandType.GetUptime, null, "tester");
        var result = await service.ExecuteAsync(request, CancellationToken.None);

        Assert.That(result.Result, Is.EqualTo("cached"));
    }

    private sealed class AllowAllAllowlist : ICommandAllowlist
    {
        public bool IsAllowed(CommandType type) => true;
    }

    private sealed class DenyAllAllowlist : ICommandAllowlist
    {
        public bool IsAllowed(CommandType type) => false;
    }

    private sealed class SuccessExecutor : ICommandExecutor
    {
        public Task<string> ExecuteAsync(CommandRequest request, CancellationToken cancellationToken) => Task.FromResult("ok");
    }

    private sealed class FakeAuditLogger : IAuditLogger
    {
        public Task WriteAsync(CommandExecution execution) => Task.CompletedTask;
    }

    private sealed class FakeRepository : ICommandRepository
    {
        private readonly Dictionary<Guid, CommandExecution> values = new();

        public Task UpsertAsync(CommandExecution execution)
        {
            values[execution.CommandId] = execution;
            return Task.CompletedTask;
        }

        public Task<CommandExecution?> GetAsync(Guid commandId)
        {
            values.TryGetValue(commandId, out var execution);
            return Task.FromResult(execution);
        }
    }

    private sealed class FakeEventPublisher : IEventPublisher
    {
        public Task PublishAsync(string topic, string payload, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
