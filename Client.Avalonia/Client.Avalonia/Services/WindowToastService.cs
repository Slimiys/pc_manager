using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;

namespace Client.Avalonia.Services;

/// <summary>
/// Toast-уведомления на базе <see cref="WindowNotificationManager"/> (Fluent theme).
/// </summary>
public sealed class WindowToastService : IToastService
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(5);

    private WindowNotificationManager? _manager;

    /// <inheritdoc />
    public void Attach(TopLevel topLevel)
    {
        _manager = new WindowNotificationManager(topLevel)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 5
        };
    }

    /// <inheritdoc />
    public void ShowInformation(string title, string? message = null)
    {
        if (_manager is null)
        {
            return;
        }

        void Show()
        {
            var body = message ?? string.Empty;
            var notification = new Notification(title, body, NotificationType.Information, DefaultDuration);
            _manager.Show(notification);
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Show();
        }
        else
        {
            Dispatcher.UIThread.Post(Show);
        }
    }
}
