namespace Server.Infrastructure.Execution;

/// <summary>
/// Настройки подключения к host-агенту.
/// </summary>
public sealed class AgentOptions
{
    /// <summary>
    /// Базовый URL агента.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// API ключ для внутренней авторизации между сервером и агентом.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
}
