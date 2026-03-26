namespace Server.Application;

/// <summary>
/// DTO с базовой системной информацией.
/// </summary>
public sealed record SystemInfoDto(string MachineName, string OsDescription, string Framework, DateTimeOffset UtcNow);
