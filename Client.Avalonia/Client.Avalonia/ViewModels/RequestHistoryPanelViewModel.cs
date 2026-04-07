using System.Collections.ObjectModel;
using Client.Avalonia.Localization;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// ViewModel истории запросов.
/// </summary>
public sealed class RequestHistoryPanelViewModel : ViewModelBase, IDisposable
{
    private readonly ILocalizationService _localization;
    private readonly SourceList<string> _historySource = new();
    private readonly IDisposable _cleanUp;

    /// <summary>
    /// Создаёт ViewModel истории запросов.
    /// </summary>
    /// <param name="localization">Сервис локализации.</param>
    public RequestHistoryPanelViewModel(ILocalizationService localization)
    {
        _localization = localization;
        _localization.CultureChanged += OnLocalizationCultureChanged;

        _cleanUp = _historySource.Connect()
            .Bind(out var historyItems)
            .Subscribe();
        HistoryItems = historyItems;
    }

    /// <summary>
    /// Коллекция истории запросов.
    /// </summary>
    public ReadOnlyObservableCollection<string> HistoryItems { get; }

    /// <summary>
    /// Заголовок секции истории.
    /// </summary>
    public string SectionTitle => _localization.GetString(UiStringKeys.History.LabelRecentRequests);

    /// <summary>
    /// Добавляет запись в историю.
    /// </summary>
    /// <param name="entry">Текст записи.</param>
    public void AddEntry(string entry)
    {
        _historySource.Add(entry);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _localization.CultureChanged -= OnLocalizationCultureChanged;
        _cleanUp.Dispose();
        _historySource.Dispose();
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(SectionTitle));
    }
}

