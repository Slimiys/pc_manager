using Avalonia.Controls;
using Avalonia.Threading;
using Client.ToastListener.Services;

namespace Client.ToastListener.Desktop.Services;

/// <summary>
/// Показ уведомлений через центр уведомлений Windows (вне окна приложения).
/// Использует <see cref="ToastNotificationManagerCompat"/> — для неупакованного Win32 он регистрирует COM и AppUserModelId;
/// прямой вызов <c>Windows.UI.Notifications.ToastNotificationManager</c> без этого обычно не показывает toast.
/// </summary>
public sealed class WindowsSystemToastService : IToastService
{
    /// <inheritdoc />
    public void Attach(TopLevel topLevel)
    {
        // Системные toast не привязаны к визуальному дереву Avalonia.
    }

    /// <inheritdoc />
    public void ShowInformation(string title, string? message = null)
    {
        void Show()
        {
            ToastNotificationPresenter.Show(title, message);
        }

        // API уведомлений ожидает STA; при вызове с потока Kestrel переключаемся на UI-поток.
        // Invoke (не Post), чтобы ошибка была видна в логе до ответа HTTP и не терялась асинхронно.
        if (Dispatcher.UIThread.CheckAccess())
        {
            Show();
        }
        else
        {
            Dispatcher.UIThread.Invoke(Show);
        }
    }
}
