using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Client.Avalonia.Localization;
using Client.Avalonia.Services;

namespace Client.Avalonia.Desktop.Services;

/// <summary>
/// Поддержка сворачивания главного окна в системный трей.
/// </summary>
public sealed class MainWindowTraySupport : IDisposable
{
    private readonly Window _window;
    private readonly ILocalizationService _localization;
    private readonly IClassicDesktopStyleApplicationLifetime _desktopLifetime;
    private readonly TrayIcon _trayIcon;
    private readonly TrayIcons _trayIcons;
    private readonly NativeMenuItem _showMenuItem;
    private readonly NativeMenuItem _exitMenuItem;
    private bool _isDisposed;
    private bool _isExplicitShutdown;

    /// <summary>
    /// Создаёт и подключает иконку трея к главному окну.
    /// </summary>
    /// <param name="window">Главное окно приложения.</param>
    /// <param name="localization">Сервис локализации.</param>
    /// <param name="desktopLifetime">Жизненный цикл desktop-приложения.</param>
    public MainWindowTraySupport(
        Window window,
        ILocalizationService localization,
        IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        _window = window;
        _localization = localization;
        _desktopLifetime = desktopLifetime;

        _showMenuItem = new NativeMenuItem();
        _exitMenuItem = new NativeMenuItem();

        _trayIcon = new TrayIcon
        {
            Icon = ResolveTrayIcon(window),
            IsVisible = true,
            Menu = new NativeMenu
            {
                Items =
                {
                    _showMenuItem,
                    new NativeMenuItemSeparator(),
                    _exitMenuItem
                }
            }
        };

        _trayIcons = [_trayIcon];
        ApplyLocalizedTexts();
        if (Application.Current is { } application)
        {
            TrayIcon.SetIcons(application, _trayIcons);
        }

        _showMenuItem.Click += OnShowMenuItemClick;
        _exitMenuItem.Click += OnExitMenuItemClick;
        _trayIcon.Clicked += OnTrayIconClicked;
        _localization.CultureChanged += OnLocalizationCultureChanged;

        _window.Closing += OnWindowClosing;
        _window.PropertyChanged += OnWindowPropertyChanged;
        _window.Closed += OnWindowClosed;

        _desktopLifetime.ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    /// <summary>
    /// Регистрирует фабрику в общем приложении.
    /// </summary>
    public static void Register()
    {
        MainWindowTraySupportFactory.Create = static (window, localization, desktopLifetime) =>
        {
            _ = new MainWindowTraySupport(window, localization, desktopLifetime);
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _localization.CultureChanged -= OnLocalizationCultureChanged;
        _window.Closing -= OnWindowClosing;
        _window.PropertyChanged -= OnWindowPropertyChanged;
        _window.Closed -= OnWindowClosed;
        _showMenuItem.Click -= OnShowMenuItemClick;
        _exitMenuItem.Click -= OnExitMenuItemClick;
        _trayIcon.Clicked -= OnTrayIconClicked;

        _trayIcon.IsVisible = false;
        if (Application.Current is { } application)
        {
            TrayIcon.SetIcons(application, null);
        }
        _trayIcon.Dispose();
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (_isExplicitShutdown)
        {
            return;
        }

        e.Cancel = true;
        HideToTray();
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_isExplicitShutdown)
        {
            return;
        }

        if (e.Property == Window.WindowStateProperty
            && e.NewValue is WindowState.Minimized)
        {
            HideToTray();
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e) => Dispose();

    private void OnTrayIconClicked(object? sender, EventArgs e) => ShowFromTray();

    private void OnShowMenuItemClick(object? sender, EventArgs e) => ShowFromTray();

    private void OnExitMenuItemClick(object? sender, EventArgs e) => ExitApplication();

    private void OnLocalizationCultureChanged(object? sender, EventArgs e) => ApplyLocalizedTexts();

    private void HideToTray()
    {
        _window.WindowState = WindowState.Normal;
        _window.Hide();
    }

    private void ShowFromTray()
    {
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void ExitApplication()
    {
        _isExplicitShutdown = true;
        _trayIcon.IsVisible = false;
        _desktopLifetime.Shutdown();
    }

    private void ApplyLocalizedTexts()
    {
        _trayIcon.ToolTipText = _localization.GetString(UiStringKeys.Common.AppTitle);
        _showMenuItem.Header = _localization.GetString(UiStringKeys.Common.TrayShowWindow);
        _exitMenuItem.Header = _localization.GetString(UiStringKeys.Common.TrayExit);
    }

    private static WindowIcon ResolveTrayIcon(Window window)
    {
        if (window.Icon is not null)
        {
            return window.Icon;
        }

        return new WindowIcon(AssetLoader.Open(new Uri("avares://Client.Avalonia/Assets/avalonia-logo.ico")));
    }
}
