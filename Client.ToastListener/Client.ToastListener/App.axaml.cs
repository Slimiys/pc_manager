using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Client.ToastListener.Services;

namespace Client.ToastListener;

/// <summary>
/// Точка входа Avalonia: минимальное окно и привязка toast.
/// </summary>
public partial class App : Application
{
    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var toastService = ToastListenerBootstrap.CreateToastService?.Invoke() ?? new WindowToastService();
            desktop.MainWindow = new MainWindow(toastService);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
