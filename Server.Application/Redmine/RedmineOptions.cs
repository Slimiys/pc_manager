namespace Server.Application.Redmine;

/// <summary>
/// Настройки опроса Redmine REST API.
/// </summary>
public sealed class RedmineOptions
{
    /// <summary>Включён ли фоновый опрос Redmine.</summary>
    public bool Enabled { get; init; }

    /// <summary>Базовый URL Redmine без завершающего слэша.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>API-ключ Redmine.</summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Фильтр project_id (идентификатор или slug проекта).</summary>
    public string? ProjectId { get; init; }

    /// <summary>Фильтр fixed_version_id.</summary>
    public string? FixedVersionId { get; init; }

    /// <summary>Интервал опроса в секундах.</summary>
    public int PollIntervalSeconds { get; init; } = 60;

    /// <summary>Максимум задач в одном запросе.</summary>
    public int IssueFetchLimit { get; init; } = 10;

    /// <summary>Таймаут подключения HTTP, секунды.</summary>
    public int ConnectTimeoutSeconds { get; init; } = 10;

    /// <summary>Таймаут чтения HTTP, секунды.</summary>
    public int ReadTimeoutSeconds { get; init; } = 30;

    /// <summary>Путь к JSON-файлу состояния seen issue → status id.</summary>
    public string StateFilePath { get; init; } = "data/redmine_seen_issues.json";

    /// <summary>Отправлять оповещения (toast и общий канал) при обнаружении изменений.</summary>
    public bool NotificationsEnabled { get; init; } = true;

    /// <summary>
    /// Возвращает абсолютный путь к файлу состояния.
    /// </summary>
    /// <param name="contentRoot">Корень контента приложения.</param>
    public string ResolveStateFilePath(string contentRoot)
    {
        if (Path.IsPathRooted(StateFilePath))
        {
            return StateFilePath;
        }

        return Path.Combine(contentRoot, StateFilePath);
    }
}
