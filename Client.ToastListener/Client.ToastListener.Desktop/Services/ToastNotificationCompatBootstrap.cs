using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace Client.ToastListener.Desktop.Services;

/// <summary>
/// Единоразовая инициализация ToastNotificationManagerCompat (должна выполняться на STA-потоке, который будет показывать toast).
/// </summary>
internal static class ToastNotificationCompatBootstrap
{
    private static volatile bool _initialized;

    /// <summary>
    /// Регистрирует COM/AppUserModelId для неупакованного Win32; безопасно вызывать повторно.
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
            Log.Information("ToastNotificationManagerCompat: готов к показу уведомлений.");
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "ToastNotificationManagerCompat: инициализация не удалась; уведомления могут не отображаться.");
        }
    }
}
