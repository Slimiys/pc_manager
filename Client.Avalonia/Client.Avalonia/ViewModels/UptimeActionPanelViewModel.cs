using System.Reactive;
using Client.Avalonia.Localization;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// ViewModel панели действия запроса uptime.
/// </summary>
public sealed class UptimeActionPanelViewModel : ViewModelBase
{
    private string _statusKey = UiStringKeys.Common.StatusReady;
    private object?[]? _statusArgs;
    private bool _isBusy;
    private string _agentAvailabilityKey = UiStringKeys.Common.AgentAvailabilityChecking;
    private ReactiveCommand<Unit, Unit>? _getUptimeCommand;
    private ReactiveCommand<Unit, Unit>? _lockWorkstationCommand;
    private ReactiveCommand<Unit, Unit>? _sendTestNotificationCommand;
    private ReactiveCommand<Unit, Unit>? _setPowerPlanOfficeCommand;
    private ReactiveCommand<Unit, Unit>? _setPowerPlanGamingCommand;
    private ReactiveCommand<Unit, Unit>? _setPowerPlanPerformanceCommand;

    /// <summary>
    /// Создаёт панель действий.
    /// </summary>
    public UptimeActionPanelViewModel()
    {
    }

    /// <summary>
    /// Ключ текущего статуса выполнения операции.
    /// </summary>
    public string StatusKey
    {
        get => _statusKey;
        set => this.RaiseAndSetIfChanged(ref _statusKey, value);
    }

    /// <summary>
    /// Аргументы форматирования для статуса локализации.
    /// </summary>
    public object?[]? StatusArgs
    {
        get => _statusArgs;
        set => this.RaiseAndSetIfChanged(ref _statusArgs, value);
    }

    /// <summary>
    /// Признак активного запроса.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    /// <summary>
    /// Ключ статуса доступности агента.
    /// </summary>
    public string AgentAvailabilityKey
    {
        get => _agentAvailabilityKey;
        set => this.RaiseAndSetIfChanged(ref _agentAvailabilityKey, value);
    }

    /// <summary>
    /// Команда выполнения GetUptime.
    /// </summary>
    public ReactiveCommand<Unit, Unit>? GetUptimeCommand
    {
        get => _getUptimeCommand;
        set => this.RaiseAndSetIfChanged(ref _getUptimeCommand, value);
    }

    /// <summary>
    /// Команда выполнения LockWorkstation.
    /// </summary>
    public ReactiveCommand<Unit, Unit>? LockWorkstationCommand
    {
        get => _lockWorkstationCommand;
        set => this.RaiseAndSetIfChanged(ref _lockWorkstationCommand, value);
    }

    /// <summary>
    /// Команда отправки тестового оповещения.
    /// </summary>
    public ReactiveCommand<Unit, Unit>? SendTestNotificationCommand
    {
        get => _sendTestNotificationCommand;
        set => this.RaiseAndSetIfChanged(ref _sendTestNotificationCommand, value);
    }

    /// <summary>
    /// Команда установки схемы питания Office.
    /// </summary>
    public ReactiveCommand<Unit, Unit>? SetPowerPlanOfficeCommand
    {
        get => _setPowerPlanOfficeCommand;
        set => this.RaiseAndSetIfChanged(ref _setPowerPlanOfficeCommand, value);
    }

    /// <summary>
    /// Команда установки схемы питания Gaming.
    /// </summary>
    public ReactiveCommand<Unit, Unit>? SetPowerPlanGamingCommand
    {
        get => _setPowerPlanGamingCommand;
        set => this.RaiseAndSetIfChanged(ref _setPowerPlanGamingCommand, value);
    }

    /// <summary>
    /// Команда установки схемы питания Performance.
    /// </summary>
    public ReactiveCommand<Unit, Unit>? SetPowerPlanPerformanceCommand
    {
        get => _setPowerPlanPerformanceCommand;
        set => this.RaiseAndSetIfChanged(ref _setPowerPlanPerformanceCommand, value);
    }
}

