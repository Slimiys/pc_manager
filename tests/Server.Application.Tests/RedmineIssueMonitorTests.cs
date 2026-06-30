using Server.Application.Redmine;

namespace Server.Application.Tests;

/// <summary>
/// Тесты детекции изменений задач Redmine.
/// </summary>
public sealed class RedmineIssueMonitorTests
{
    private readonly RedmineIssueMonitor _monitor = new();

    [Test]
    public void DetectChanges_NewIssue_ReturnsNewIssueEvent()
    {
        var issue = CreateIssue(101, 1, "Новая", "Test task");
        var result = _monitor.DetectChanges([issue], new Dictionary<string, int>(), "https://redmine.example");

        Assert.That(result.Events, Has.Count.EqualTo(1));
        Assert.That(result.Events[0].Kind, Is.EqualTo(RedmineIssueChangeKind.NewIssue));
        Assert.That(result.Events[0].IssueId, Is.EqualTo(101));
        Assert.That(result.UpdatedSeenState["101"], Is.EqualTo(1));
    }

    [Test]
    public void DetectChanges_ResolvedIssueOnFirstSight_IsSkipped()
    {
        var issue = CreateIssue(102, 5, "Решена", "Done task");
        var result = _monitor.DetectChanges([issue], new Dictionary<string, int>(), "https://redmine.example");

        Assert.That(result.Events, Is.Empty);
        Assert.That(result.UpdatedSeenState, Does.Not.ContainKey("102"));
    }

    [Test]
    public void DetectChanges_StatusChanged_ReturnsStatusChangedEvent()
    {
        var issue = CreateIssue(103, 2, "В работе", "In progress");
        var seen = new Dictionary<string, int> { ["103"] = 1 };
        var result = _monitor.DetectChanges([issue], seen, "https://redmine.example");

        Assert.That(result.Events, Has.Count.EqualTo(1));
        Assert.That(result.Events[0].Kind, Is.EqualTo(RedmineIssueChangeKind.StatusChanged));
        Assert.That(result.UpdatedSeenState["103"], Is.EqualTo(2));
    }

    private static RedmineIssue CreateIssue(int id, int statusId, string statusName, string subject) =>
        new()
        {
            Id = id,
            Subject = subject,
            StatusId = statusId,
            StatusName = statusName,
            ProjectName = "Demo",
            Description = "Description"
        };
}
