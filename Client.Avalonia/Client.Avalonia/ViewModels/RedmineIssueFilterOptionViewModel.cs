namespace Client.Avalonia.ViewModels;

/// <summary>
/// Пункт меню фильтра колонки таблицы задач.
/// </summary>
public sealed class RedmineIssueFilterOptionViewModel
{
    /// <summary>
    /// Создаёт пункт меню фильтра.
    /// </summary>
    /// <param name="label">Текст пункта.</param>
    /// <param name="value">Значение фильтра или null для «все».</param>
    /// <param name="isSelected">Выбран ли пункт.</param>
    public RedmineIssueFilterOptionViewModel(string label, string? value, bool isSelected)
    {
        Label = label;
        Value = value;
        IsSelected = isSelected;
    }

    /// <summary>Отображаемый текст.</summary>
    public string Label { get; }

    /// <summary>Значение фильтра; null — без фильтрации.</summary>
    public string? Value { get; }

    /// <summary>Текущий выбранный пункт.</summary>
    public bool IsSelected { get; }

    /// <summary>Текст с отметкой выбранного пункта.</summary>
    public string DisplayLabel => IsSelected ? $"✓ {Label}" : Label;
}
