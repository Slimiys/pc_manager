using Client.Avalonia.Localization;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// ViewModel панели результата запроса uptime.
/// </summary>
public sealed class UptimeResultPanelViewModel : ViewModelBase, IDisposable
{
    private readonly ILocalizationService _localization;
    private string? _uptimeResult;

    /// <summary>
    /// Создаёт панель результата с локализованными заголовками.
    /// </summary>
    /// <param name="localization">Сервис локализации.</param>
    public UptimeResultPanelViewModel(ILocalizationService localization)
    {
        _localization = localization;
        _localization.CultureChanged += OnLocalizationCultureChanged;
    }

    /// <summary>
    /// Текст результата с сервера (или ошибки); если <see langword="null"/>, показывается заполнитель.
    /// </summary>
    public string? UptimeResult
    {
        get => _uptimeResult;
        set
        {
            this.RaiseAndSetIfChanged(ref _uptimeResult, value);
            this.RaisePropertyChanged(nameof(ResultDisplay));
        }
    }

    /// <summary>
    /// Текст для отображения: результат или локализованный заполнитель «нет данных».
    /// </summary>
    public string ResultDisplay => UptimeResult ?? _localization.GetString(UiStringKeys.PlaceholderNoData);

    /// <summary>
    /// Заголовок секции результата.
    /// </summary>
    public string SectionTitle => _localization.GetString(UiStringKeys.LabelCurrentUptimeResult);

    /// <inheritdoc />
    public void Dispose()
    {
        _localization.CultureChanged -= OnLocalizationCultureChanged;
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(ResultDisplay));
        this.RaisePropertyChanged(nameof(SectionTitle));
    }
}
