using Avalonia.Controls;
using Client.Avalonia.Services;

namespace Client.Avalonia.Views;

public partial class MainView : UserControl
{
    /// <summary>
    /// Конструктор для превью в дизайнере (без toast).
    /// </summary>
    public MainView()
        : this(NullToastService.Instance)
    {
    }

    /// <summary>
    /// Создаёт главное представление с привязкой toast к <see cref="TopLevel"/>.
    /// </summary>
    public MainView(IToastService toastService)
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                toastService.Attach(topLevel);
            }
        };
    }
}