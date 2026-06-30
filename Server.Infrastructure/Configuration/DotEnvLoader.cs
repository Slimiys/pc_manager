namespace Server.Infrastructure.Configuration;

/// <summary>
/// Результат загрузки файла .env.
/// </summary>
public sealed class DotEnvLoadResult
{
    /// <summary>
    /// Создаёт результат загрузки.
    /// </summary>
    /// <param name="loaded">Файл найден и обработан.</param>
    /// <param name="filePath">Путь к файлу или null.</param>
    /// <param name="appliedRedmineKeys">Число применённых ключей Redmine.</param>
    public DotEnvLoadResult(bool loaded, string? filePath, int appliedRedmineKeys)
    {
        Loaded = loaded;
        FilePath = filePath;
        AppliedRedmineKeys = appliedRedmineKeys;
    }

    /// <summary>Файл .env найден и прочитан.</summary>
    public bool Loaded { get; }

    /// <summary>Абсолютный путь к .env.</summary>
    public string? FilePath { get; }

    /// <summary>Число переменных Redmine, записанных в окружение.</summary>
    public int AppliedRedmineKeys { get; }
}

/// <summary>
/// Загрузка переменных из .env в окружение процесса (совместимость с redmine_notify.py).
/// </summary>
public static class DotEnvLoader
{
    private static readonly IReadOnlyDictionary<string, string> RedmineEnvironmentMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["REDMINE_URL"] = "Redmine__BaseUrl",
            ["API_KEY"] = "Redmine__ApiKey",
            ["PROJECT_ID"] = "Redmine__ProjectId",
            ["FIXED_VERSION_ID"] = "Redmine__FixedVersionId",
            ["CHECK_INTERVAL"] = "Redmine__PollIntervalSeconds",
            ["REDMINE_ENABLED"] = "Redmine__Enabled"
        };

    /// <summary>
    /// Ищет .env, читает его и применяет переменные Redmine к конфигурации ASP.NET Core.
    /// </summary>
    /// <param name="startDirectory">Каталог начала поиска (обычно ContentRoot или BaseDirectory).</param>
    /// <param name="explicitFilePath">Явный путь к .env или null для автопоиска.</param>
    public static DotEnvLoadResult TryApplyRedmineFromFile(
        string? startDirectory = null,
        string? explicitFilePath = null)
    {
        var envPath = ResolveEnvFilePath(startDirectory, explicitFilePath);
        if (envPath is null)
        {
            return new DotEnvLoadResult(false, null, 0);
        }

        var variables = ReadVariables(envPath);
        var applied = ApplyRedmineVariables(variables);
        return new DotEnvLoadResult(true, envPath, applied);
    }

    /// <summary>
    /// Ищет файл .env вверх по дереву каталогов от указанной точки.
    /// </summary>
    /// <param name="startDirectory">Начальный каталог.</param>
    /// <param name="explicitFilePath">Явный путь или null.</param>
    public static string? ResolveEnvFilePath(string? startDirectory, string? explicitFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitFilePath))
        {
            var full = Path.GetFullPath(explicitFilePath);
            return File.Exists(full) ? full : null;
        }

        var fromEnv = Environment.GetEnvironmentVariable("PC_MONITOR_ENV_FILE");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            var full = Path.GetFullPath(fromEnv);
            return File.Exists(full) ? full : null;
        }

        var directory = string.IsNullOrWhiteSpace(startDirectory)
            ? Directory.GetCurrentDirectory()
            : startDirectory;

        var current = new DirectoryInfo(directory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// Читает пары ключ-значение из .env.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    public static IReadOnlyDictionary<string, string> ReadVariables(string filePath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            if (!TryParseLine(rawLine, out var key, out var value))
            {
                continue;
            }

            result[key] = value;
        }

        return result;
    }

    private static int ApplyRedmineVariables(IReadOnlyDictionary<string, string> variables)
    {
        var applied = 0;
        foreach (var (sourceKey, targetKey) in RedmineEnvironmentMap)
        {
            if (!variables.TryGetValue(sourceKey, out var rawValue))
            {
                continue;
            }

            var normalized = NormalizeRedmineValue(sourceKey, rawValue);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(targetKey)))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(targetKey, normalized);
            applied++;
        }

        return applied;
    }

    private static string NormalizeRedmineValue(string sourceKey, string value)
    {
        var trimmed = Unquote(value.Trim());
        if (string.Equals(sourceKey, "REDMINE_URL", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed.TrimEnd('/');
        }

        return trimmed;
    }

    private static bool TryParseLine(string rawLine, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;

        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
        {
            return false;
        }

        if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
        {
            line = line[7..].TrimStart();
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            return false;
        }

        key = line[..separatorIndex].Trim();
        value = line[(separatorIndex + 1)..].Trim();
        return key.Length > 0;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2)
        {
            if ((value.StartsWith('"') && value.EndsWith('"'))
                || (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                return value[1..^1];
            }
        }

        return value;
    }
}
