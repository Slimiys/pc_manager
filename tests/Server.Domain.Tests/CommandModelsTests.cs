using Server.Domain.Commands;

namespace Server.Domain.Tests;

public sealed class CommandModelsTests
{
    [Test]
    public void CommandRequest_ShouldKeepData()
    {
        var id = Guid.NewGuid();
        var model = new CommandRequest(id, CommandType.GetUptime, "payload", "user");

        Assert.That(model.CommandId, Is.EqualTo(id));
        Assert.That(model.Type, Is.EqualTo(CommandType.GetUptime));
        Assert.That(model.Payload, Is.EqualTo("payload"));
        Assert.That(model.RequestedBy, Is.EqualTo("user"));
    }
}
