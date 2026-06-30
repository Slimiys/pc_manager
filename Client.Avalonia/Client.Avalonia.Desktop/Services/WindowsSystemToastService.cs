using Avalonia.Controls;
using Avalonia.Threading;
using Client.Avalonia.Services;

namespace Client.Avalonia.Desktop.Services;

/// <summary>
/// Показ уведомлений через центр уведомлений Windows (вне окна приложения).
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
