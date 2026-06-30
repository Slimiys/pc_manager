namespace Server.Application.Redmine;

/// <summary>
/// Событие по задаче Redmine для публикации оповещения и UI.
/// </summary>
public sealed class RedmineIssueChangeEvent
{
    /// <summary>Идентификатор задачи.</summary>
    public int IssueId { get; init; }

    /// <summary>Тип события.</summary>
    public RedmineIssueChangeKind Kind { get; init; }

    /// <summary>Заголовок оповещения.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Текст оповещения.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Тема задачи.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Название статуса.</summary>
    public string StatusName { get; init; } = string.Empty;

    /// <summary>Отображаемый статус с эмодзи.</summary>
    public string StatusWithEmoji { get; init; } = string.Empty;

    /// <summary>Название проекта.</summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Название версии или null.</summary>
    public string? VersionName { get; init; }

    /// <summary>URL задачи в Redmine.</summary>
    public string IssueUrl { get; init; } = string.Empty;

    /// <summary>Дата создания задачи UTC.</summary>
    public DateTimeOffset CreatedOnUtc { get; init; }

    /// <summary>Дата изменения задачи UTC.</summary>
    public DateTimeOffset UpdatedOnUtc { get; init; }
}
