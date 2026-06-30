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

        /// <summary>Ключ заголовка приложения.</summary>
        public const string AppTitle = "CommonStrings.AppTitle";

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

        /// <summary>Ключ вкладки PC Manager.</summary>
        public const string TabPcManager = "CommonStrings.TabPcManager";

        /// <summary>Ключ вкладки Redmine.</summary>
        public const string TabRedmine = "CommonStrings.TabRedmine";

        /// <summary>Ключ пункта меню трея «Показать окно».</summary>
        public const string TrayShowWindow = "CommonStrings.TrayShowWindow";

        /// <summary>Ключ пункта меню трея «Выход».</summary>
        public const string TrayExit = "CommonStrings.TrayExit";
    }

    #endregion

    #region Redmine

    /// <summary>
    /// Ключи строк вкладки Redmine.
    /// </summary>
    public static class Redmine
    {
        /// <summary>Ключ подписи статуса мониторинга.</summary>
        public const string LabelMonitoringStatus = "RedmineStrings.LabelMonitoringStatus";

        /// <summary>Ключ подписи переключателя мониторинга.</summary>
        public const string LabelEnableMonitoring = "RedmineStrings.LabelEnableMonitoring";

        /// <summary>Ключ заголовка последнего события.</summary>
        public const string LabelLastEvent = "RedmineStrings.LabelLastEvent";

        /// <summary>Ключ подписи времени получения события.</summary>
        public const string LabelEventReceivedAt = "RedmineStrings.LabelEventReceivedAt";

        /// <summary>Ключ подписи номера задачи в событии.</summary>
        public const string LabelEventIssueId = "RedmineStrings.LabelEventIssueId";

        /// <summary>Ключ подписи версии в событии.</summary>
        public const string LabelEventVersion = "RedmineStrings.LabelEventVersion";

        /// <summary>Ключ подписи проекта в событии.</summary>
        public const string LabelEventProject = "RedmineStrings.LabelEventProject";

        /// <summary>Ключ подписи темы в событии.</summary>
        public const string LabelEventSubject = "RedmineStrings.LabelEventSubject";

        /// <summary>Ключ подписи даты в событии.</summary>
        public const string LabelEventDate = "RedmineStrings.LabelEventDate";

        /// <summary>Ключ подписи статуса в событии.</summary>
        public const string LabelEventStatus = "RedmineStrings.LabelEventStatus";

        /// <summary>Ключ пояснения при отсутствии событий.</summary>
        public const string DetailNoLastEvent = "RedmineStrings.DetailNoLastEvent";

        /// <summary>Ключ кнопки загрузки последних задач.</summary>
        public const string ButtonFetchLatestIssues = "RedmineStrings.ButtonFetchLatestIssues";

        /// <summary>Ключ подписи поля количества задач.</summary>
        public const string LabelIssueFetchLimit = "RedmineStrings.LabelIssueFetchLimit";

        /// <summary>Ключ индикатора загрузки задач.</summary>
        public const string LabelFetchingIssues = "RedmineStrings.LabelFetchingIssues";

        /// <summary>Ключ заголовка колонки номера задачи.</summary>
        public const string ColumnIssueId = "RedmineStrings.ColumnIssueId";

        /// <summary>Ключ заголовка колонки статуса.</summary>
        public const string ColumnStatus = "RedmineStrings.ColumnStatus";

        /// <summary>Ключ заголовка колонки приоритета.</summary>
        public const string ColumnPriority = "RedmineStrings.ColumnPriority";

        /// <summary>Ключ заголовка колонки версии.</summary>
        public const string ColumnVersion = "RedmineStrings.ColumnVersion";

        /// <summary>Ключ заголовка колонки даты.</summary>
        public const string ColumnDate = "RedmineStrings.ColumnDate";

        /// <summary>Ключ заголовка колонки описания.</summary>
        public const string ColumnSubject = "RedmineStrings.ColumnSubject";

        /// <summary>Ключ пункта «все» в меню фильтра.</summary>
        public const string FilterAll = "RedmineStrings.FilterAll";

        /// <summary>Ключ статуса «проверка».</summary>
        public const string StatusChecking = "RedmineStrings.StatusChecking";

        /// <summary>Ключ статуса «подключено».</summary>
        public const string StatusConnected = "RedmineStrings.StatusConnected";

        /// <summary>Ключ статуса «нет связи».</summary>
        public const string StatusDisconnected = "RedmineStrings.StatusDisconnected";

        /// <summary>Ключ статуса «отключено на сервере».</summary>
        public const string StatusDisabled = "RedmineStrings.StatusDisabled";

        /// <summary>Ключ пояснения при отключённом мониторинге.</summary>
        public const string DetailMonitoringOff = "RedmineStrings.DetailMonitoringOff";

        /// <summary>Ключ ошибки при смене состояния переключателя.</summary>
        public const string DetailToggleFailed = "RedmineStrings.DetailToggleFailed";

        /// <summary>Ключ пояснения при ожидании первого опроса.</summary>
        public const string DetailWaitingForPoll = "RedmineStrings.DetailWaitingForPoll";

        /// <summary>Ключ пояснения при недоступности сервера.</summary>
        public const string DetailServerUnavailable = "RedmineStrings.DetailServerUnavailable";

        /// <summary>Ключ формата числа отслеживаемых задач.</summary>
        public const string DetailTrackedIssuesFormat = "RedmineStrings.DetailTrackedIssuesFormat";
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

        /// <summary>Ключ кнопки схемы питания Office.</summary>
        public const string ButtonPowerPlanOffice = "CommandStrings.ButtonPowerPlanOffice";

        /// <summary>Ключ кнопки схемы питания Gaming.</summary>
        public const string ButtonPowerPlanGaming = "CommandStrings.ButtonPowerPlanGaming";

        /// <summary>Ключ кнопки схемы питания Performance.</summary>
        public const string ButtonPowerPlanPerformance = "CommandStrings.ButtonPowerPlanPerformance";

        /// <summary>Ключ отображаемого имени команды SetPowerPlanOffice.</summary>
        public const string CommandDisplay_SetPowerPlanOffice = "CommandStrings.CommandDisplay_SetPowerPlanOffice";

        /// <summary>Ключ отображаемого имени команды SetPowerPlanGaming.</summary>
        public const string CommandDisplay_SetPowerPlanGaming = "CommandStrings.CommandDisplay_SetPowerPlanGaming";

        /// <summary>Ключ отображаемого имени команды SetPowerPlanPerformance.</summary>
        public const string CommandDisplay_SetPowerPlanPerformance = "CommandStrings.CommandDisplay_SetPowerPlanPerformance";
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
