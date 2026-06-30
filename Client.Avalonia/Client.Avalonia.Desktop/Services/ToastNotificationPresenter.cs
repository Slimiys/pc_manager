using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace Client.Avalonia.Desktop.Services;

/// <summary>
/// Показ системного toast в центре уведомлений Windows.
/// </summary>
internal static class ToastNotificationPresenter
{
    /// <summary>
    /// Показывает toast с заданными заголовком и текстом.
    /// </summary>
    public static void Show(string title, string? message)
    {
        try
        {
            var builder = new ToastContentBuilder().AddText(title);
            if (!string.IsNullOrWhiteSpace(message))
            {
                builder.AddText(message);
            }

            // Уникальный Tag — иначе Windows заменяет предыдущее уведомление тем же приложением.
            builder.Show(toast => toast.Tag = Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Не удалось показать системное уведомление Windows.");
        }
    }
}
