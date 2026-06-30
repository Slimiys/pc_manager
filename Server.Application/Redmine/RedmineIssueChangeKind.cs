namespace Server.Application.Redmine;

/// <summary>
/// Тип события по задаче Redmine.
/// </summary>
public enum RedmineIssueChangeKind
{
    /// <summary>Новая задача в выборке.</summary>
    NewIssue = 1,

    /// <summary>Изменился статус задачи.</summary>
    StatusChanged = 2
}
