using System.Globalization;
using Client.Avalonia.Localization;
using Client.Avalonia.Services;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// Корневая ViewModel оболочки с вкладками PC Manager и Redmine.
/// </summary>
public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private const int RedmineTabIndex = 1;

    private readonly AuthApiClient _authApiClient;
    private readonly TokenCache _tokenCache;
    private readonly ILocalizationService _localization;

    private LanguageRowViewModel _selectedLanguage;
    private int _selectedTabIndex;

    /// <summary>
    /// Создаёт корневую ViewModel приложения.
    /// </summary>
    public MainViewModel(
        AuthApiClient authApiClient,
        CommandsApiClient commandsApiClient,
        NotificationsApiClient notificationsApiClient,
        RedmineApiClient redmineApiClient,
        TokenCache tokenCache,
        ILocalizationService localization,
        IToastService toastService)
    {
        _authApiClient = authApiClient;
        _tokenCache = tokenCache;
        _localization = localization;
        _localization.CultureChanged += OnLocalizationCultureChanged;

        Func<CancellationToken, Task<string>> ensureTokenAsync = EnsureTokenAsync;
        PcManager = new PcManagerTabViewModel(
            commandsApiClient,
            notificationsApiClient,
            localization,
            toastService,
            ensureTokenAsync);
        Redmine = new RedmineTabViewModel(
            redmineApiClient,
            localization,
            ensureTokenAsync);

        LanguageRows =
        [
            new LanguageRowViewModel(localization, new CultureInfo("en"), UiStringKeys.Common.LanguageEnglish),
            new LanguageRowViewModel(localization, new CultureInfo("ru"), UiStringKeys.Common.LanguageRussian)
        ];

        _selectedLanguage = ResolveInitialLanguageRow();
    }

    /// <summary>
    /// Индекс выбранной вкладки.
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
            if (value == RedmineTabIndex)
            {
                Redmine.EnsureMonitoringEnabled();
            }
        }
    }

    /// <summary>
    /// Вкладка управления ПК.
    /// </summary>
    public PcManagerTabViewModel PcManager { get; }

    /// <summary>
    /// Вкладка Redmine.
    /// </summary>
    public RedmineTabViewModel Redmine { get; }

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
            if (value == null || ReferenceEquals(_selectedLanguage, value))
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

    private async Task<string> EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (!_tokenCache.HasToken)
        {
            var token = await _authApiClient.GetTokenAsync(cancellationToken);
            _tokenCache.SetToken(token);
        }

        var tokenValue = _tokenCache.GetToken();
        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            throw new InvalidOperationException(_localization.GetString(UiStringKeys.Common.ErrorTokenMissing));
        }

        return tokenValue;
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        SyncSelectedLanguageFromService();
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

    /// <inheritdoc />
    public void Dispose()
    {
        _localization.CultureChanged -= OnLocalizationCultureChanged;
        PcManager.Dispose();
        Redmine.Dispose();
    }
}
