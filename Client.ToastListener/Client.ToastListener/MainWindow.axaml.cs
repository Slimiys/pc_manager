using Avalonia.Controls;
using Client.ToastListener.Services;

namespace Client.ToastListener;

/// <summary>
/// Минимальное окно для привязки <see cref="WindowNotificationManager"/> к <see cref="TopLevel"/>.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Конструктор для превью в дизайнере.
    /// </summary>
    public MainWindow()
        : this(new WindowToastService())
    {
    }

    /// <summary>
    /// Создаёт окно и привязывает toast после загрузки.
    /// </summary>
    /// <param name="toastService">Сервис toast.</param>
    public MainWindow(IToastService toastService)
    {
        InitializeComponent();
        Title = ToastListenerApplication.WindowTitle;

        var key = ToastListenerDisplayInfo.ExpectedApiKey;
        KeyHintTextBlock.Text = string.IsNullOrWhiteSpace(key)
            ? "ToastListener:ApiKey не задан в конфигурации."
            : $"Заголовок X-Toast-Listener-Key (как Notifications:ToastListenerKey на сервере):\n{key}\n\nСлушаем: {ToastListenerDisplayInfo.ListenAddress}";

        Loaded += (_, _) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is not null)
            {
                toastService.Attach(topLevel);
            }

            ToastListenerBootstrap.OnWindowReady?.Invoke(toastService);
        };
    }
}
