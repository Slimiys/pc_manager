using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace Client.Avalonia.Desktop.Services;

/// <summary>
/// Единоразовая инициализация ToastNotificationManagerCompat для неупакованного Win32.
/// </summary>
internal static class ToastNotificationCompatBootstrap
{
    private static volatile bool _initialized;

    /// <summary>
    /// Регистрирует COM/AppUserModelId; безопасно вызывать повторно.
    /// </summary>
    public static void TryInitialize()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            _ = ToastNotificationManagerCompat.CreateToastNotifier();
            _initialized = true;
            Log.Information("ToastNotificationManagerCompat: готов к показу уведомлений Windows.");
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "ToastNotificationManagerCompat: инициализация не удалась.");
        }
    }
}
