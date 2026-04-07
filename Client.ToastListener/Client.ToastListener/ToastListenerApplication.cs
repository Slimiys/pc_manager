using System.Reflection;

namespace Client.ToastListener;

/// <summary>
/// Метаданные приложения: заголовок окна и отображаемое имя в системных уведомлениях Windows
/// должны совпадать с атрибутами сборки исполняемого файла (см. Desktop csproj).
/// </summary>
public static class ToastListenerApplication
{
    private const string FallbackWindowTitle = "PcManager Toast Listener";

    /// <summary>
    /// Заголовок главного окна; тот же текст Windows использует как подпись приложения в toast (через Win32AppInfo / DisplayName).
    /// </summary>
    public static string WindowTitle
    {
        get
        {
            try
            {
                var entry = Assembly.GetEntryAssembly();
                var fromTitle = entry?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
                if (!string.IsNullOrWhiteSpace(fromTitle))
                {
                    return fromTitle;
                }

                var fromProduct = entry?.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
                if (!string.IsNullOrWhiteSpace(fromProduct))
                {
                    return fromProduct;
                }
            }
            catch
            {
                // Превью в дизайнере и нестандартные домены загрузки.
            }

            return FallbackWindowTitle;
        }
    }
}
