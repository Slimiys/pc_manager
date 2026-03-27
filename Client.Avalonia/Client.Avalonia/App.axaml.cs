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
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(apiSettings.BaseUrl.EndsWith("/")
                ? apiSettings.BaseUrl
                : $"{apiSettings.BaseUrl}/")
        };
        var tokenCache = new TokenCache();
        var authApiClient = new AuthApiClient(httpClient, apiSettings);
        var commandsApiClient = new CommandsApiClient(httpClient);
        var mainViewModel = new MainViewModel(authApiClient, commandsApiClient, tokenCache, localizationService);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Убираем дублирующую data-валидацию Avalonia-плагина.
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
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