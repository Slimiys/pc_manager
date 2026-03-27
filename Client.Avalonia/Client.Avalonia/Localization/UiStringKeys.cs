namespace Client.Avalonia.Localization;

/// <summary>
/// Ключи строк интерфейса в нескольких RESX-словарях.
/// </summary>
public static class UiStringKeys
{
    /// <summary>Ключ заголовка приложения.</summary>
    public const string AppTitle = "CommonStrings.AppTitle";

    /// <summary>Ключ подписи кнопки получения времени работы.</summary>
    public const string ButtonGetUptime = "CommandStrings.ButtonGetUptime";

    /// <summary>Ключ подписи кнопки блокировки рабочей станции.</summary>
    public const string ButtonLockWorkstation = "CommandStrings.ButtonLockWorkstation";

    /// <summary>Ключ заголовка блока результата.</summary>
    public const string LabelCurrentUptimeResult = "CommandStrings.LabelCurrentUptimeResult";

    /// <summary>Ключ заголовка блока истории.</summary>
    public const string LabelRecentRequests = "HistoryStrings.LabelRecentRequests";

    /// <summary>Ключ текста-заполнителя при отсутствии данных.</summary>
    public const string PlaceholderNoData = "CommonStrings.PlaceholderNoData";

    /// <summary>Ключ статуса «готово».</summary>
    public const string StatusReady = "CommonStrings.StatusReady";

    /// <summary>Ключ статуса «выполняется» (шаблон с аргументом).</summary>
    public const string StatusProcessing = "CommonStrings.StatusProcessing";

    /// <summary>Ключ статуса «успешно».</summary>
    public const string StatusSuccess = "CommonStrings.StatusSuccess";

    /// <summary>Ключ статуса «ошибка».</summary>
    public const string StatusFailed = "CommonStrings.StatusFailed";

    /// <summary>Ключ подписи выбора языка.</summary>
    public const string LabelLanguage = "CommonStrings.LabelLanguage";

    /// <summary>Ключ названия языка English.</summary>
    public const string LanguageEnglish = "CommonStrings.LanguageEnglish";

    /// <summary>Ключ названия языка Russian.</summary>
    public const string LanguageRussian = "CommonStrings.LanguageRussian";

    /// <summary>Ключ отображаемого имени команды GetUptime.</summary>
    public const string CommandDisplay_GetUptime = "CommandStrings.CommandDisplay_GetUptime";

    /// <summary>Ключ отображаемого имени команды LockWorkstation.</summary>
    public const string CommandDisplay_LockWorkstation = "CommandStrings.CommandDisplay_LockWorkstation";

    /// <summary>Ключ формата строки истории успеха.</summary>
    public const string HistoryEntryFormat = "HistoryStrings.HistoryEntryFormat";

    /// <summary>Ключ формата строки истории ошибки.</summary>
    public const string HistoryErrorFormat = "HistoryStrings.HistoryErrorFormat";

    /// <summary>Ключ ошибки отсутствия токена.</summary>
    public const string ErrorTokenMissing = "CommonStrings.ErrorTokenMissing";
}
