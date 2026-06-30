using Server.Application.Redmine;

namespace Server.Api.Models;

/// <summary>
/// Событие Redmine для клиентского UI.
/// </summary>
public sealed record RedmineEventDto(
    int IssueId,
    string Kind,
    string Title,
    string Message,
    string Subject,
    string StatusName,
    string StatusWithEmoji,
    string ProjectName,
    string? VersionName,
    string IssueUrl,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset UpdatedOnUtc,
    DateTimeOffset ReceivedAtUtc);
