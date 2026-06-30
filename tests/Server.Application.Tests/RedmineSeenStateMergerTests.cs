using Server.Application.Redmine;

namespace Server.Application.Tests;

/// <summary>
/// Тесты тихого слияния состояния задач Redmine.
/// </summary>
public sealed class RedmineSeenStateMergerTests
{
    [Test]
    public void MergeFetchedIssues_AddsNewIssuesAndUpdatesExisting()
    {
        var seen = new Dictionary<string, int> { ["1"] = 10 };
        var issues = new[]
        {
            CreateIssue(1, statusId: 20),
            CreateIssue(2, statusId: 30)
        };

        var merged = RedmineSeenStateMerger.MergeFetchedIssues(seen, issues);

        Assert.That(merged, Has.Count.EqualTo(2));
        Assert.That(merged["1"], Is.EqualTo(20));
        Assert.That(merged["2"], Is.EqualTo(30));
    }

    [Test]
    public void MergeFetchedIssues_SkipsResolvedIssues()
    {
        var seen = new Dictionary<string, int> { ["1"] = 10 };
        var issues = new[] { CreateIssue(2, statusId: 30, statusName: "Решена") };

        var merged = RedmineSeenStateMerger.MergeFetchedIssues(seen, issues);

        Assert.That(merged, Has.Count.EqualTo(1));
        Assert.That(merged, Does.ContainKey("1"));
        Assert.That(merged, Does.Not.ContainKey("2"));
    }

    private static RedmineIssue CreateIssue(int id, int statusId = 1, string statusName = "Новая") =>
        new()
        {
            Id = id,
            Subject = "Тема",
            StatusId = statusId,
            StatusName = statusName,
            ProjectName = "Проект"
        };
}
