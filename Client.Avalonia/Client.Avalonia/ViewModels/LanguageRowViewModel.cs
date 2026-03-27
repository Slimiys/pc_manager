using System.Globalization;
using Client.Avalonia.Localization;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// Элемент списка выбора языка интерфейса (культура + локализуемая подпись).
/// </summary>
public sealed class LanguageRowViewModel : ViewModelBase, IDisposable
{
    private readonly ILocalizationService _localization;
    private readonly string _displayNameResourceKey;

    /// <summary>
    /// Создаёт элемент выбора языка.
    /// </summary>
    /// <param name="localization">Сервис локализации.</param>
    /// <param name="culture">Культура, соответствующая пункту списка.</param>
    /// <param name="displayNameResourceKey">Ключ строки названия языка в RESX.</param>
    public LanguageRowViewModel(
        ILocalizationService localization,
        CultureInfo culture,
        string displayNameResourceKey)
    {
        _localization = localization;
        Culture = culture;
        _displayNameResourceKey = displayNameResourceKey;
        _localization.CultureChanged += OnLocalizationCultureChanged;
    }

    /// <summary>
    /// Культура данного пункта.
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// Отображаемое имя языка с учётом текущей локали интерфейса.
    /// </summary>
    public string DisplayName => _localization.GetString(_displayNameResourceKey);

    /// <inheritdoc />
    public void Dispose()
    {
        _localization.CultureChanged -= OnLocalizationCultureChanged;
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(DisplayName));
    }
}
