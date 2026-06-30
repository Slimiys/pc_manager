namespace Server.Application.Redmine;

/// <summary>
/// Задача Redmine, полученная из REST API.
/// </summary>
public sealed class RedmineIssue
{
    /// <summary>Идентификатор задачи.</summary>
    public int Id { get; init; }

    /// <summary>Тема задачи.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Описание задачи.</summary>
    public string? Description { get; init; }

    /// <summary>Идентификатор статуса.</summary>
    public int StatusId { get; init; }

    /// <summary>Название статуса.</summary>
    public string StatusName { get; init; } = string.Empty;

    /// <summary>Название приоритета.</summary>
    public string PriorityName { get; init; } = string.Empty;

    /// <summary>Название проекта.</summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Название версии/спринта или null.</summary>
    public string? VersionName { get; init; }

    /// <summary>Дата создания задачи (UTC).</summary>
    public DateTimeOffset CreatedOnUtc { get; init; }

    /// <summary>Дата последнего изменения задачи (UTC).</summary>
    public DateTimeOffset UpdatedOnUtc { get; init; }
}
