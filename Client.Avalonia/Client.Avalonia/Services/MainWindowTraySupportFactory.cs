using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Client.Avalonia.Localization;

namespace Client.Avalonia.Services;

/// <summary>
/// Фабрика подключения сворачивания главного окна в системный трей (desktop).
/// </summary>
public static class MainWindowTraySupportFactory
{
    /// <summary>
    /// Создаёт поддержку трея для главного окна; null на платформах без трея.
    /// </summary>
    public static Action<Window, ILocalizationService, IClassicDesktopStyleApplicationLifetime>? Create { get; set; }
}
