using System.Globalization;
using System.ComponentModel;

namespace Client.Avalonia.Localization;

/// <summary>
/// Источник данных для привязок локализации в XAML.
/// </summary>
public sealed class LocalizationBindingSource : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationBindingSource> InstanceHolder = new(() => new LocalizationBindingSource());
    private ILocalizationService? _localizationService;
    private int _version;

    private LocalizationBindingSource()
    {
    }

    /// <summary>
    /// Возвращает singleton-экземпляр источника локализации.
    /// </summary>
    public static LocalizationBindingSource Instance => InstanceHolder.Value;

    /// <summary>
    /// Счётчик смены языка для принудительного обновления MultiBinding.
    /// </summary>
    public int Version => _version;

    /// <summary>
    /// Текущая культура локализации.
    /// </summary>
    public CultureInfo CurrentCulture => _localizationService?.CurrentCulture ?? CultureInfo.InvariantCulture;

    /// <summary>
    /// Индексатор для получения перевода по ключу.
    /// </summary>
    /// <param name="resourceKey">Ключ ресурса.</param>
    /// <returns>Локализованная строка или сам ключ.</returns>
    public string this[string resourceKey] => Translate(resourceKey);

    /// <summary>
    /// Событие изменения данных источника.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Инициализирует источник конкретной реализацией сервиса локализации.
    /// </summary>
    /// <param name="localizationService">Сервис локализации.</param>
    public void Initialize(ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(localizationService);

        if (_localizationService != null)
        {
            _localizationService.CultureChanged -= OnCultureChanged;
        }

        _localizationService = localizationService;
        _localizationService.CultureChanged += OnCultureChanged;
        NotifyLocalizationChanged();
    }

    /// <summary>
    /// Выполняет перевод по ключу.
    /// </summary>
    /// <param name="resourceKey">Ключ ресурса.</param>
    /// <returns>Локализованная строка.</returns>
    public string Translate(string resourceKey)
    {
        if (_localizationService == null || string.IsNullOrWhiteSpace(resourceKey))
        {
            return resourceKey;
        }

        return _localizationService.GetString(resourceKey);
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        NotifyLocalizationChanged();
    }

    private void NotifyLocalizationChanged()
    {
        _version++;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
    }
}
