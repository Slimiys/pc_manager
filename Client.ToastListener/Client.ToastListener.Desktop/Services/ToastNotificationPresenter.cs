using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace Client.ToastListener.Desktop.Services;

/// <summary>
/// Общая логика показа системного toast (используется и из Avalonia UI-потока, и из STA WinForms).
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

            // У каждого toast должен быть свой Tag: иначе Windows считает это «тем же» уведомлением
            // и заменяет предыдущее, пока пользователь не уберёт его из центра уведомлений.
            builder.Show(toast => toast.Tag = Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Не удалось показать системное уведомление Windows (проверьте «Уведомления» и режим «Не беспокоить»).");
        }
    }
}
