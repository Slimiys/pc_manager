using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Client.Avalonia.Localization;
using Client.Avalonia.Services;
using Client.Avalonia.ViewModels;
using Client.Avalonia.Views;

namespace Client.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var localizationService = new LocalizationService();
        ApplyStartupUiCulture(localizationService);
        LocalizationBindingSource.Instance.Initialize(localizationService);

        var apiSettings = new ApiSettings();
        var resolvedBaseUrl = ResolveBaseUrl(apiSettings);
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(resolvedBaseUrl.EndsWith("/")
                ? resolvedBaseUrl
                : $"{resolvedBaseUrl}/")
        };
        var tokenCache = new TokenCache();
        var authApiClient = new AuthApiClient(httpClient, apiSettings);
        var commandsApiClient = new CommandsApiClient(httpClient);
        var notificationsApiClient = new NotificationsApiClient(httpClient, apiSettings);
        var toastService = new WindowToastService();
        var mainViewModel = new MainViewModel(
            authApiClient,
            commandsApiClient,
            notificationsApiClient,
            tokenCache,
            localizationService,
            toastService);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Убираем дублирующую data-валидацию Avalonia-плагина.
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow(toastService)
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView(toastService)
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string ResolveBaseUrl(ApiSettings apiSettings)
    {
        if (!apiSettings.EnableLanDiscovery)
        {
            return apiSettings.BaseUrl;
        }

        try
        {
            var isLocalPlaceholder = IsLocalPlaceholderHost(apiSettings.BaseUrl);
            if (!isLocalPlaceholder)
            {
                return apiSettings.BaseUrl;
            }

            var discoveryClient = new LanDiscoveryClient();
            var discovered = discoveryClient
                .TryDiscoverBaseUrlAsync(apiSettings, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            return string.IsNullOrWhiteSpace(discovered) ? apiSettings.BaseUrl : discovered;
        }
        catch
        {
            return apiSettings.BaseUrl;
        }
    }

    private static bool IsLocalPlaceholderHost(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Host, "0.0.0.0", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Выбирает стартовый язык UI: русский для системной локали ru, иначе английский.
    /// </summary>
    private static void ApplyStartupUiCulture(ILocalizationService localizationService)
    {
        var uiTwoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (string.Equals(uiTwoLetter, "ru", StringComparison.OrdinalIgnoreCase))
        {
            localizationService.CurrentCulture = new CultureInfo("ru");
            return;
        }

        localizationService.CurrentCulture = new CultureInfo("en");
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}