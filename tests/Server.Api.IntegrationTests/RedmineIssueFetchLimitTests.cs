using Server.Api.Services;
using Server.Application.Redmine;

namespace Server.Api.IntegrationTests;

/// <summary>
/// Тесты runtime-лимита задач Redmine.
/// </summary>
public sealed class RedmineIssueFetchLimitTests
{
    [Test]
    public void SetValue_ClampsToValidRange()
    {
        var limit = new RedmineIssueFetchLimit(new RedmineOptions { IssueFetchLimit = 10 });

        limit.SetValue(0);
        Assert.That(limit.Value, Is.EqualTo(1));

        limit.SetValue(100);
        Assert.That(limit.Value, Is.EqualTo(50));

        limit.SetValue(25);
        Assert.That(limit.Value, Is.EqualTo(25));
    }

    [Test]
    public void Constructor_UsesConfiguredDefault()
    {
        var limit = new RedmineIssueFetchLimit(new RedmineOptions { IssueFetchLimit = 30 });
        Assert.That(limit.Value, Is.EqualTo(30));
    }
}
