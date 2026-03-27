using System.Reactive;
using Client.Avalonia.Localization;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// ViewModel панели действия запроса uptime.
/// </summary>
public sealed class UptimeActionPanelViewModel : ViewModelBase
{
    private string _statusKey = UiStringKeys.StatusReady;
    private object?[]? _statusArgs;
    private bool _isBusy;
    private ReactiveCommand<Unit, Unit>? _getUptimeCommand;
    private ReactiveCommand<Unit, Unit>? _lockWorkstationCommand;

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
}
