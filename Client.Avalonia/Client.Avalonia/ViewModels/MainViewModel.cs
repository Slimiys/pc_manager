using System.Globalization;
using System.Reactive;
using Avalonia.Threading;
using Client.Avalonia.Localization;
using Client.Avalonia.Services;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// Главная ViewModel экрана управления командами.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    private readonly AuthApiClient _authApiClient;
    private readonly CommandsApiClient _commandsApiClient;
    private readonly TokenCache _tokenCache;
    private readonly ILocalizationService _localization;

    private string _statusResourceKey = UiStringKeys.StatusReady;
    private string? _processingCommandName;
    private LanguageRowViewModel _selectedLanguage;

    /// <summary>
    /// Создает главную ViewModel.
    /// </summary>
    public MainViewModel(
        AuthApiClient authApiClient,
        CommandsApiClient commandsApiClient,
        TokenCache tokenCache,
        ILocalizationService localization)
    {
        _authApiClient = authApiClient;
        _commandsApiClient = commandsApiClient;
        _tokenCache = tokenCache;
        _localization = localization;
        _localization.CultureChanged += OnLocalizationCultureChanged;

        ActionPanel = new UptimeActionPanelViewModel();
        ResultPanel = new UptimeResultPanelViewModel(localization);
        HistoryPanel = new RequestHistoryPanelViewModel(localization);

        LanguageRows =
        [
            new LanguageRowViewModel(localization, new CultureInfo("en"), UiStringKeys.LanguageEnglish),
            new LanguageRowViewModel(localization, new CultureInfo("ru"), UiStringKeys.LanguageRussian)
        ];

        _selectedLanguage = ResolveInitialLanguageRow();

        ExecuteGetUptimeCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteGetUptimeAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        ExecuteLockWorkstationCommand = ReactiveCommand.CreateFromTask(
            execute: ExecuteLockWorkstationAsync,
            outputScheduler: RxApp.MainThreadScheduler);

        ActionPanel.GetUptimeCommand = ExecuteGetUptimeCommand;
        ActionPanel.LockWorkstationCommand = ExecuteLockWorkstationCommand;

        ActionPanel.StatusKey = UiStringKeys.StatusReady;
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

    /// <summary>
    /// Доступные языки интерфейса.
    /// </summary>
    public IReadOnlyList<LanguageRowViewModel> LanguageRows { get; }

    /// <summary>
    /// Выбранный язык интерфейса.
    /// </summary>
    public LanguageRowViewModel SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value == null)
            {
                return;
            }

            if (ReferenceEquals(_selectedLanguage, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedLanguage, value);

            if (!CultureEquals(value.Culture, _localization.CurrentCulture))
            {
                _localization.CurrentCulture = value.Culture;
            }
        }
    }

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

    private async Task ExecuteCommandWithUiFlowAsync(string commandName, Func<string, Task<string>> execute)
    {
        _statusResourceKey = UiStringKeys.StatusProcessing;
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
            var tokenValue = await EnsureTokenAsync();
            var result = await execute(tokenValue);
            _statusResourceKey = UiStringKeys.StatusSuccess;
            _processingCommandName = null;

            var displayName = _localization.GetString(GetCommandDisplayResourceKey(commandName));
            UpdateUi(() =>
            {
                ResultPanel.UptimeResult = result;
                ActionPanel.StatusKey = _statusResourceKey;
                ActionPanel.StatusArgs = null;
                HistoryPanel.AddEntry(
                    _localization.GetString(
                        UiStringKeys.HistoryEntryFormat,
                        DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        displayName,
                        result));
            });
        }
        catch (Exception ex)
        {
            _statusResourceKey = UiStringKeys.StatusFailed;
            _processingCommandName = null;

            var displayName = _localization.GetString(GetCommandDisplayResourceKey(commandName));
            UpdateUi(() =>
            {
                ActionPanel.StatusKey = _statusResourceKey;
                ActionPanel.StatusArgs = null;
                ResultPanel.UptimeResult = ex.Message;
                HistoryPanel.AddEntry(
                    _localization.GetString(
                        UiStringKeys.HistoryErrorFormat,
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

    private async Task<string> EnsureTokenAsync()
    {
        if (!_tokenCache.HasToken)
        {
            var token = await _authApiClient.GetTokenAsync(CancellationToken.None);
            _tokenCache.SetToken(token);
        }

        var tokenValue = _tokenCache.GetToken();
        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            throw new InvalidOperationException(_localization.GetString(UiStringKeys.ErrorTokenMissing));
        }

        return tokenValue;
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        SyncSelectedLanguageFromService();
        if (!string.Equals(_statusResourceKey, UiStringKeys.StatusProcessing, StringComparison.Ordinal)
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

    private LanguageRowViewModel ResolveInitialLanguageRow()
    {
        var match = LanguageRows.FirstOrDefault(row => CultureEquals(row.Culture, _localization.CurrentCulture));
        return match ?? LanguageRows[0];
    }

    private void SyncSelectedLanguageFromService()
    {
        var match = LanguageRows.FirstOrDefault(row => CultureEquals(row.Culture, _localization.CurrentCulture));
        if (match != null && !ReferenceEquals(_selectedLanguage, match))
        {
            this.RaiseAndSetIfChanged(ref _selectedLanguage, match);
        }
    }

    private static bool CultureEquals(CultureInfo left, CultureInfo right)
    {
        return string.Equals(
            left.TwoLetterISOLanguageName,
            right.TwoLetterISOLanguageName,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCommandDisplayResourceKey(string commandName)
    {
        return commandName switch
        {
            "GetUptime" => UiStringKeys.CommandDisplay_GetUptime,
            "LockWorkstation" => UiStringKeys.CommandDisplay_LockWorkstation,
            _ => commandName
        };
    }

    private static void UpdateUi(Action updateAction)
    {
        Dispatcher.UIThread.Post(updateAction);
    }
}
