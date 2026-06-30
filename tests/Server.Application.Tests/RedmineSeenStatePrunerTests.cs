using Server.Application.Redmine;

namespace Server.Application.Tests;

/// <summary>
/// Тесты обрезки состояния отслеживаемых задач Redmine.
/// </summary>
public sealed class RedmineSeenStatePrunerTests
{
    [Test]
    public void PruneToFetchedIssues_KeepsOnlyFetchedIssueIds()
    {
        var seen = new Dictionary<string, int>
        {
            ["1"] = 10,
            ["2"] = 20,
            ["99"] = 30
        };
        var issues = new[]
        {
            CreateIssue(1),
            CreateIssue(2)
        };

        var pruned = RedmineSeenStatePruner.PruneToFetchedIssues(seen, issues);

        Assert.That(pruned, Has.Count.EqualTo(2));
        Assert.That(pruned, Does.ContainKey("1"));
        Assert.That(pruned, Does.ContainKey("2"));
        Assert.That(pruned, Does.Not.ContainKey("99"));
    }

    private static RedmineIssue CreateIssue(int id) =>
        new()
        {
            Id = id,
            Subject = "Тема",
            StatusId = 1,
            StatusName = "Новая",
            ProjectName = "Проект"
        };
}
