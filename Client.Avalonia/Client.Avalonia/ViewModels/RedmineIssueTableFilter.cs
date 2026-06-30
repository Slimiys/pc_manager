namespace Client.Avalonia.ViewModels;

/// <summary>
/// Состояние фильтров таблицы задач Redmine.
/// </summary>
internal sealed class RedmineIssueTableFilter
{
    /// <summary>Фильтр по статусу или null.</summary>
    public string? Status { get; set; }

    /// <summary>Фильтр по приоритету или null.</summary>
    public string? Priority { get; set; }

    /// <summary>Фильтр по версии или null.</summary>
    public string? Version { get; set; }

    /// <summary>
    /// Проверяет, подходит ли строка под активные фильтры.
    /// </summary>
    public bool Matches(RedmineIssueRowViewModel row) =>
        (Status is null || row.StatusName == Status)
        && (Priority is null || row.PriorityName == Priority)
        && (Version is null || row.VersionName == Version);
}
