namespace Server.Api.Models;

/// <summary>
/// Задача Redmine для отображения в клиентском UI.
/// </summary>
public sealed record RedmineIssueDto(
    int Id,
    string Subject,
    string StatusName,
    string PriorityName,
    string? VersionName,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset UpdatedOnUtc,
    string IssueUrl);
