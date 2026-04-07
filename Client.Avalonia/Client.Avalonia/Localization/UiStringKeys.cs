namespace Client.Avalonia.Localization;

/// <summary>
/// Ключи строк интерфейса в нескольких RESX-словарях.
/// </summary>
public static class UiStringKeys
{
    #region Common

    /// <summary>
    /// Ключи общих строк интерфейса.
    /// </summary>
    public static class Common
    {
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

        /// <summary>Ключ подписи языка English.</summary>
        public const string LanguageEnglish = "CommonStrings.LanguageEnglish";

        /// <summary>Ключ подписи языка Russian.</summary>
        public const string LanguageRussian = "CommonStrings.LanguageRussian";

        /// <summary>Ключ ошибки при отсутствии токена.</summary>
        public const string ErrorTokenMissing = "CommonStrings.ErrorTokenMissing";

        /// <summary>Ключ подписи статуса доступности агента.</summary>
        public const string LabelAgentAvailability = "CommonStrings.LabelAgentAvailability";

        /// <summary>Ключ статуса «проверка доступности агента».</summary>
        public const string AgentAvailabilityChecking = "CommonStrings.AgentAvailabilityChecking";

        /// <summary>Ключ статуса «агент доступен».</summary>
        public const string AgentAvailabilityOnline = "CommonStrings.AgentAvailabilityOnline";

        /// <summary>Ключ статуса «агент недоступен».</summary>
        public const string AgentAvailabilityOffline = "CommonStrings.AgentAvailabilityOffline";
    }

    #endregion

    #region Command

    /// <summary>
    /// Ключи строк, связанных с командами.
    /// </summary>
    public static class Command
    {
        /// <summary>Ключ заголовка блока результата.</summary>
        public const string LabelCurrentUptimeResult = "CommandStrings.LabelCurrentUptimeResult";

        /// <summary>Ключ отображаемого имени команды GetUptime.</summary>
        public const string CommandDisplay_GetUptime = "CommandStrings.CommandDisplay_GetUptime";

        /// <summary>Ключ отображаемого имени команды LockWorkstation.</summary>
        public const string CommandDisplay_LockWorkstation = "CommandStrings.CommandDisplay_LockWorkstation";

        /// <summary>Ключ кнопки отправки тестового оповещения.</summary>
        public const string ButtonSendTestNotification = "CommandStrings.ButtonSendTestNotification";

        /// <summary>Ключ отображаемого имени команды отправки тестового оповещения.</summary>
        public const string CommandDisplay_SendTestNotification = "CommandStrings.CommandDisplay_SendTestNotification";
    }

    #endregion

    #region History

    /// <summary>
    /// Ключи строк истории запросов.
    /// </summary>
    public static class History
    {
        /// <summary>Ключ заголовка блока истории.</summary>
        public const string LabelRecentRequests = "HistoryStrings.LabelRecentRequests";

        /// <summary>Ключ формата строки истории успеха.</summary>
        public const string HistoryEntryFormat = "HistoryStrings.HistoryEntryFormat";

        /// <summary>Ключ формата строки истории ошибки.</summary>
        public const string HistoryErrorFormat = "HistoryStrings.HistoryErrorFormat";

        /// <summary>Ключ формата строки входящего HTTP-оповещения.</summary>
        public const string HistoryNotificationFormat = "HistoryStrings.HistoryNotificationFormat";
    }

    #endregion
}
