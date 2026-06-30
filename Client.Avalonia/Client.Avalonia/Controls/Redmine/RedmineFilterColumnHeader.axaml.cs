using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Client.Avalonia.ViewModels;

namespace Client.Avalonia.Controls.Redmine;

/// <summary>
/// Кликабельный заголовок колонки с меню фильтрации.
/// </summary>
public partial class RedmineFilterColumnHeader : Button
{
    /// <summary>
    /// Свойство заголовка колонки.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<RedmineFilterColumnHeader, string>(nameof(Title));

    /// <summary>
    /// Свойство пунктов меню фильтра.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<RedmineIssueFilterOptionViewModel>?> OptionsProperty =
        AvaloniaProperty.Register<RedmineFilterColumnHeader, IEnumerable<RedmineIssueFilterOptionViewModel>?>(nameof(Options));

    /// <summary>
    /// Свойство команды выбора значения фильтра.
    /// </summary>
    public static readonly StyledProperty<ICommand?> FilterSelectCommandProperty =
        AvaloniaProperty.Register<RedmineFilterColumnHeader, ICommand?>(nameof(FilterSelectCommand));

    /// <summary>
    /// Свойство признака активного фильтра.
    /// </summary>
    public static readonly StyledProperty<bool> IsFilterActiveProperty =
        AvaloniaProperty.Register<RedmineFilterColumnHeader, bool>(nameof(IsFilterActive));

    /// <summary>
    /// Создаёт заголовок колонки с фильтром.
    /// </summary>
    public RedmineFilterColumnHeader()
    {
        InitializeComponent();
    }

    /// <summary>Событие перед открытием меню фильтра.</summary>
    public event EventHandler? FlyoutOpening;

    /// <summary>Текст заголовка.</summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Пункты меню.</summary>
    public IEnumerable<RedmineIssueFilterOptionViewModel>? Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    /// <summary>Команда выбора фильтра.</summary>
    public ICommand? FilterSelectCommand
    {
        get => GetValue(FilterSelectCommandProperty);
        set => SetValue(FilterSelectCommandProperty, value);
    }

    /// <summary>Фильтр применён.</summary>
    public bool IsFilterActive
    {
        get => GetValue(IsFilterActiveProperty);
        set => SetValue(IsFilterActiveProperty, value);
    }

    private void OnFlyoutOpening(object? sender, EventArgs e)
    {
        FlyoutOpening?.Invoke(this, EventArgs.Empty);
    }
}
