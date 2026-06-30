using Server.Domain.Commands;

namespace Server.Domain.Tests;

/// <summary>
/// Тесты соответствия GUID схем питания командам и боту.
/// </summary>
public sealed class PowerPlanGuidsTests
{
    [Test]
    public void TryGetForCommand_SetPowerPlanOffice_ReturnsBotOfficeGuid()
    {
        var ok = PowerPlanGuids.TryGetForCommand(CommandType.SetPowerPlanOffice, out var guid);

        Assert.That(ok, Is.True);
        Assert.That(guid, Is.EqualTo("5da85d85-eb0c-4710-a805-6da8bbcf1f02"));
    }

    [Test]
    public void TryGetForCommand_SetPowerPlanGaming_ReturnsBotGamingGuid()
    {
        var ok = PowerPlanGuids.TryGetForCommand(CommandType.SetPowerPlanGaming, out var guid);

        Assert.That(ok, Is.True);
        Assert.That(guid, Is.EqualTo("653fbb7f-248b-41d0-aa49-03fabbfd5e8e"));
    }

    [Test]
    public void TryGetForCommand_SetPowerPlanPerformance_ReturnsBotPerformanceGuid()
    {
        var ok = PowerPlanGuids.TryGetForCommand(CommandType.SetPowerPlanPerformance, out var guid);

        Assert.That(ok, Is.True);
        Assert.That(guid, Is.EqualTo("a9932169-f1fb-4dff-9034-fe1c4ca1886e"));
    }

    [Test]
    public void TryGetForCommand_GetUptime_ReturnsFalse()
    {
        var ok = PowerPlanGuids.TryGetForCommand(CommandType.GetUptime, out var guid);

        Assert.That(ok, Is.False);
        Assert.That(guid, Is.Null);
    }
}
