using System.Globalization;
using System.Reactive;
using Avalonia.Threading;
using Client.Avalonia.Localization;
using Client.Avalonia.Services;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// ViewModel вкладки управления ПК.
/// </summary>
public sealed class PcManagerTabViewModel : ViewModelBase, IDisposable
{
    private const string TestNotificationTitle = "PcManager Test Notification";
    private const string TestNotificationMessage = "This is a fixed test notification sent from UI to agent.";

    private readonly CommandsApiClient _commandsApiClient;
    private readonly NotificationsApiClient _notificationsApiClient;
    private readonly ILocalizationService _localization;
    private readonly PollingNotificationSubscriber _notificationSubscriber;
    private readonly AgentAvailabilityMonitor _agentAvailabilityMonitor;
    private readonly Func<CancellationToken, Task<string>> _ensureTokenAsync;

    private string _statusResourceKey = UiStringKeys.Common.StatusReady;
    private string? _processingCommandName;

    /// <summary>
    /// Создаёт ViewModel вкладки PC Manager.
    /// </summary>
    public PcManagerTabViewModel(
        CommandsApiClient commandsApiClient,
        NotificationsApiClient notificationsApiClient,
        ILocalizationService localization,
        IToastService toastService,
        Func<CancellationToken, Task<string>> ensureTokenAsync)
    {
        _commandsApiClient = commandsApiClient;
        _notificationsApiClient = notificationsApiClient;
        _localization = localization;
        _ensureTokenAsync = ensureTokenAsync;
        _localization.CultureChanged += OnLocalizationCultureChanged;

        ActionPanel = new UptimeActionPanelViewModel();
        ResultPanel = new UptimeResultPanelViewModel(localization);
        HistoryPanel = new RequestHistoryPanelViewModel(localization);

        _notificationSubscriber = new PollingNotificationSubscriber(
            notificationsApiClient,
            ensureTokenAsync,
            localization,
            toastService,
            entry => HistoryPanel.AddEntry(entry));
        _notificationSubscriber.Start();
        _agentAvailabilityMonitor = new AgentAvailabilityMonitor(
            _commandsApiClient,
            ensureTokenAsync,
            availabilityKey => UpdateUi(() => { ActionPanel.AgentAvailabilityKey = availabilityKey; }));
        _agentAvailabilityMonitor.Start();

        ExecuteGetUptimeCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteGetUptimeAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        ExecuteLockWorkstationCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteLockWorkstationAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        ExecuteSendTestNotificationCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteSendTestNotificationAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        ExecuteSetPowerPlanOfficeCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteSetPowerPlanOfficeAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        ExecuteSetPowerPlanGamingCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteSetPowerPlanGamingAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        ExecuteSetPowerPlanPerformanceCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteSetPowerPlanPerformanceAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        ActionPanel.GetUptimeCommand = ExecuteGetUptimeCommand;
        ActionPanel.LockWorkstationCommand = ExecuteLockWorkstationCommand;
        ActionPanel.SendTestNotificationCommand = ExecuteSendTestNotificationCommand;
        ActionPanel.SetPowerPlanOfficeCommand = ExecuteSetPowerPlanOfficeCommand;
        ActionPanel.SetPowerPlanGamingCommand = ExecuteSetPowerPlanGamingCommand;
        ActionPanel.SetPowerPlanPerformanceCommand = ExecuteSetPowerPlanPerformanceCommand;

        ActionPanel.StatusKey = UiStringKeys.Common.StatusReady;
        ActionPanel.StatusArgs = null;
    }

    /// <summary>
    /// Команда выполнения GetUptime.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExecuteGetUptimeCommand { get; }

    /// <summary>
    /// Команда выполнения LockWorkstation.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExecuteLockWorkstationCommand { get; }

    /// <summary>
    /// Команда отправки тестового оповещения агенту.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExecuteSendTestNotificationCommand { get; }

    /// <summary>
    /// Команда смены схемы питания на Office.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExecuteSetPowerPlanOfficeCommand { get; }

    /// <summary>
    /// Команда смены схемы питания на Gaming.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExecuteSetPowerPlanGamingCommand { get; }

    /// <summary>
    /// Команда смены схемы питания на Performance.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ExecuteSetPowerPlanPerformanceCommand { get; }

    /// <summary>
    /// Панель действия.
    /// </summary>
    public UptimeActionPanelViewModel ActionPanel { get; }

    /// <summary>
    /// Панель результата.
    /// </summary>
    public UptimeResultPanelViewModel ResultPanel { get; }

    /// <summary>
    /// Панель истории.
    /// </summary>
    public RequestHistoryPanelViewModel HistoryPanel { get; }

    private async Task ExecuteGetUptimeAsync()
    {
        await ExecuteCommandWithUiFlowAsync(
            commandName: "GetUptime",
            execute: token => _commandsApiClient.ExecuteGetUptimeAsync(token, CancellationToken.None));
    }

    private async Task ExecuteLockWorkstationAsync()
    {
        await ExecuteCommandWithUiFlowAsync(
            commandName: "LockWorkstation",
            execute: token => _commandsApiClient.ExecuteLockWorkstationAsync(token, CancellationToken.None));
    }

    private async Task ExecuteSetPowerPlanOfficeAsync()
    {
        await ExecuteCommandWithUiFlowAsync(
            commandName: "SetPowerPlanOffice",
            execute: token => _commandsApiClient.ExecuteSetPowerPlanOfficeAsync(token, CancellationToken.None));
    }

    private async Task ExecuteSetPowerPlanGamingAsync()
    {
        await ExecuteCommandWithUiFlowAsync(
            commandName: "SetPowerPlanGaming",
            execute: token => _commandsApiClient.ExecuteSetPowerPlanGamingAsync(token, CancellationToken.None));
    }

    private async Task ExecuteSetPowerPlanPerformanceAsync()
    {
        await ExecuteCommandWithUiFlowAsync(
            commandName: "SetPowerPlanPerformance",
            execute: token => _commandsApiClient.ExecuteSetPowerPlanPerformanceAsync(token, CancellationToken.None));
    }

    private async Task ExecuteSendTestNotificationAsync()
    {
        var commandName = "SendTestNotification";
        _statusResourceKey = UiStringKeys.Common.StatusProcessing;
        _processingCommandName = commandName;

        UpdateUi(() =>
        {
            ActionPanel.IsBusy = true;
            ActionPanel.StatusKey = _statusResourceKey;
            ActionPanel.StatusArgs =
            [
                _localization.GetString(GetCommandDisplayResourceKey(commandName))
            ];
        });

        try
        {
            await _notificationsApiClient.SendInboundAsync(
                TestNotificationTitle,
                TestNotificationMessage,
                CancellationToken.None);

            _statusResourceKey = UiStringKeys.Common.StatusSuccess;
            _processingCommandName = null;

            var displayName = _localization.GetString(GetCommandDisplayResourceKey(commandName));
            UpdateUi(() =>
            {
                ActionPanel.StatusKey = _statusResourceKey;
                ActionPanel.StatusArgs = null;
                ResultPanel.UptimeResult = $"{TestNotificationTitle} / {TestNotificationMessage}";
                HistoryPanel.AddEntry(
                    _localization.GetString(
                        UiStringKeys.History.HistoryEntryFormat,
                        DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        displayName,
                        "OK"));
            });
        }
        catch (Exception ex)
        {
            _statusResourceKey = UiStringKeys.Common.StatusFailed;
            _processingCommandName = null;

            var displayName = _localization.GetString(GetCommandDisplayResourceKey(commandName));
            UpdateUi(() =>
            {
                ActionPanel.StatusKey = _statusResourceKey;
                ActionPanel.StatusArgs = null;
                ResultPanel.UptimeResult = ex.Message;
                HistoryPanel.AddEntry(
                    _localization.GetString(
                        UiStringKeys.History.HistoryErrorFormat,
                        DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        displayName,
                        ex.Message));
            });
        }
        finally
        {
            UpdateUi(() => { ActionPanel.IsBusy = false; });
        }
    }

    private async Task ExecuteCommandWithUiFlowAsync(string commandName, Func<string, Task<string>> execute)
    {
        _statusResourceKey = UiStringKeys.Common.StatusProcessing;
        _processingCommandName = commandName;

        UpdateUi(() =>
        {
            ActionPanel.IsBusy = true;
            ActionPanel.StatusKey = _statusResourceKey;
            ActionPanel.StatusArgs =
            [
                _localization.GetString(GetCommandDisplayResourceKey(commandName))
            ];
        });

        try
        {
            var tokenValue = await _ensureTokenAsync(CancellationToken.None);
            var result = await execute(tokenValue);
            _statusResourceKey = UiStringKeys.Common.StatusSuccess;
            _processingCommandName = null;

            var displayName = _localization.GetString(GetCommandDisplayResourceKey(commandName));
            UpdateUi(() =>
            {
                ResultPanel.UptimeResult = result;
                ActionPanel.StatusKey = _statusResourceKey;
                ActionPanel.StatusArgs = null;
                HistoryPanel.AddEntry(
                    _localization.GetString(
                        UiStringKeys.History.HistoryEntryFormat,
                        DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        displayName,
                        result));
            });
        }
        catch (Exception ex)
        {
            _statusResourceKey = UiStringKeys.Common.StatusFailed;
            _processingCommandName = null;

            var displayName = _localization.GetString(GetCommandDisplayResourceKey(commandName));
            UpdateUi(() =>
            {
                ActionPanel.StatusKey = _statusResourceKey;
                ActionPanel.StatusArgs = null;
                ResultPanel.UptimeResult = ex.Message;
                HistoryPanel.AddEntry(
                    _localization.GetString(
                        UiStringKeys.History.HistoryErrorFormat,
                        DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        displayName,
                        ex.Message));
            });
        }
        finally
        {
            UpdateUi(() => { ActionPanel.IsBusy = false; });
        }
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        if (!string.Equals(_statusResourceKey, UiStringKeys.Common.StatusProcessing, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(_processingCommandName))
        {
            return;
        }

        UpdateUi(() =>
        {
            ActionPanel.StatusArgs =
            [
                _localization.GetString(GetCommandDisplayResourceKey(_processingCommandName))
            ];
        });
    }

    private static string GetCommandDisplayResourceKey(string commandName)
    {
        return commandName switch
        {
            "GetUptime" => UiStringKeys.Command.CommandDisplay_GetUptime,
            "LockWorkstation" => UiStringKeys.Command.CommandDisplay_LockWorkstation,
            "SendTestNotification" => UiStringKeys.Command.CommandDisplay_SendTestNotification,
            "SetPowerPlanOffice" => UiStringKeys.Command.CommandDisplay_SetPowerPlanOffice,
            "SetPowerPlanGaming" => UiStringKeys.Command.CommandDisplay_SetPowerPlanGaming,
            "SetPowerPlanPerformance" => UiStringKeys.Command.CommandDisplay_SetPowerPlanPerformance,
            _ => commandName
        };
    }

    private static void UpdateUi(Action updateAction)
    {
        Dispatcher.UIThread.Post(updateAction);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _agentAvailabilityMonitor.Dispose();
        _localization.CultureChanged -= OnLocalizationCultureChanged;
        _notificationSubscriber.Dispose();
        HistoryPanel.Dispose();
    }
}
