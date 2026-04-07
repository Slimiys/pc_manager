using Avalonia.Controls;
using Client.Avalonia.Services;

namespace Client.Avalonia.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Конструктор для превью в дизайнере.
    /// </summary>
    public MainWindow()
        : this(NullToastService.Instance)
    {
    }

    /// <summary>
    /// Создаёт главное окно с контентом и toast-уведомлениями.
    /// </summary>
    public MainWindow(IToastService toastService)
    {
        InitializeComponent();
        Content = new MainView(toastService);
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}