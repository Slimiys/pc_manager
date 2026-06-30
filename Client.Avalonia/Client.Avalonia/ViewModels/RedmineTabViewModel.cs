using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Threading;
using Client.Avalonia.Localization;
using Client.Avalonia.Services;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// ViewModel вкладки мониторинга Redmine.
/// </summary>
public sealed class RedmineTabViewModel : ViewModelBase, IDisposable
{
    /// <summary>Минимальное число задач за один запрос.</summary>
    public const int MinIssueFetchLimit = 1;

    /// <summary>Максимальное число задач за один запрос (ограничение API).</summary>
    public const int MaxIssueFetchLimit = 50;

    private static readonly TimeSpan IssueFetchLimitDebounce = TimeSpan.FromMilliseconds(400);

    private readonly RedmineApiClient _redmineApiClient;
    private readonly Func<CancellationToken, Task<string>> _ensureTokenAsync;
    private readonly ILocalizationService _localization;
    private readonly CancellationTokenSource _disposeCts = new();
    private readonly RedmineLastEventViewModel _lastEvent = new();
    private readonly List<RedmineIssueRowViewModel> _sourceIssueRows = new();
    private readonly RedmineIssueTableFilter _issueTableFilter = new();

    private string _statusKey = UiStringKeys.Redmine.StatusChecking;
    private string? _statusDetail;
    private bool _isMonitoringEnabled;
    private bool _isToggleBusy;
    private bool _isFetchIssuesBusy;
    private decimal _issueFetchLimit = 10;
    private bool _suppressMonitoringSync;
    private bool _suppressIssueFetchLimitSync;
    private int _syncedIssueFetchLimit = 10;
    private CancellationTokenSource? _issueFetchLimitDebounceCts;
    private CancellationTokenSource? _issueFetchLimitApplyCts;
    private DateTimeOffset _lastSeenEventUtc = DateTimeOffset.UtcNow;

    /// <summary>
    /// Создаёт ViewModel вкладки Redmine.
    /// </summary>
    public RedmineTabViewModel(
        RedmineApiClient redmineApiClient,
        ILocalizationService localization,
        Func<CancellationToken, Task<string>> ensureTokenAsync)
    {
        _redmineApiClient = redmineApiClient;
        _localization = localization;
        _ensureTokenAsync = ensureTokenAsync;
        _localization.CultureChanged += OnLocalizationCultureChanged;

        IssueRows = new ObservableCollection<RedmineIssueRowViewModel>();
        StatusFilterOptions = new ObservableCollection<RedmineIssueFilterOptionViewModel>();
        PriorityFilterOptions = new ObservableCollection<RedmineIssueFilterOptionViewModel>();
        VersionFilterOptions = new ObservableCollection<RedmineIssueFilterOptionViewModel>();

        var canFetchLatestIssues = this.WhenAnyValue(x => x.CanFetchLatestIssues);
        FetchLatestIssuesCommand = ReactiveCommand.CreateFromTask(
            FetchLatestIssuesAsync,
            canFetchLatestIssues);

        SelectStatusFilterCommand = ReactiveCommand.Create<string?>(SetStatusFilter);
        SelectPriorityFilterCommand = ReactiveCommand.Create<string?>(SetPriorityFilter);
        SelectVersionFilterCommand = ReactiveCommand.Create<string?>(SetVersionFilter);

        _ = PollLoopAsync(_disposeCts.Token);
    }

    /// <summary>Выбор фильтра по статусу.</summary>
    public ReactiveCommand<string?, Unit> SelectStatusFilterCommand { get; }

    /// <summary>Выбор фильтра по приоритету.</summary>
    public ReactiveCommand<string?, Unit> SelectPriorityFilterCommand { get; }

    /// <summary>Выбор фильтра по версии.</summary>
    public ReactiveCommand<string?, Unit> SelectVersionFilterCommand { get; }

    /// <summary>Пункты меню фильтра статуса.</summary>
    public ObservableCollection<RedmineIssueFilterOptionViewModel> StatusFilterOptions { get; }

    /// <summary>Пункты меню фильтра приоритета.</summary>
    public ObservableCollection<RedmineIssueFilterOptionViewModel> PriorityFilterOptions { get; }

    /// <summary>Пункты меню фильтра версии.</summary>
    public ObservableCollection<RedmineIssueFilterOptionViewModel> VersionFilterOptions { get; }

    /// <summary>Активен ли фильтр по статусу.</summary>
    public bool IsStatusFilterActive => _issueTableFilter.Status is not null;

    /// <summary>Активен ли фильтр по приоритету.</summary>
    public bool IsPriorityFilterActive => _issueTableFilter.Priority is not null;

    /// <summary>Активен ли фильтр по версии.</summary>
    public bool IsVersionFilterActive => _issueTableFilter.Version is not null;

    /// <summary>
    /// Последнее событие Redmine.
    /// </summary>
    public RedmineLastEventViewModel LastEvent => _lastEvent;

    /// <summary>
    /// Есть ли событие для отображения.
    /// </summary>
    public bool HasLastEvent => _lastEvent.HasEvent;

    /// <summary>
    /// Показывать ли заглушку при отсутствии событий.
    /// </summary>
    public bool ShowNoLastEvent => !_lastEvent.HasEvent;

    /// <summary>
    /// Загружает последние задачи из Redmine для проверки отображения.
    /// </summary>
    public ReactiveCommand<Unit, Unit> FetchLatestIssuesCommand { get; }

    /// <summary>
    /// Включён ли мониторинг Redmine на сервере.
    /// </summary>
    public bool IsMonitoringEnabled
    {
        get => _isMonitoringEnabled;
        set
        {
            if (_isMonitoringEnabled == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isMonitoringEnabled, value);
            if (!_suppressMonitoringSync)
            {
                _ = ApplyMonitoringToggleAsync(value);
            }
        }
    }

    /// <summary>
    /// Переключатель недоступен во время запроса к API.
    /// </summary>
    public bool IsToggleBusy
    {
        get => _isToggleBusy;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isToggleBusy, value);
            this.RaisePropertyChanged(nameof(CanToggleMonitoring));
        }
    }

    /// <summary>
    /// Переключатель доступен для взаимодействия.
    /// </summary>
    public bool CanToggleMonitoring => !IsToggleBusy;

    /// <summary>
    /// Включает мониторинг при открытии вкладки Redmine, если он выключен.
    /// </summary>
    public void EnsureMonitoringEnabled()
    {
        if (IsMonitoringEnabled || !CanToggleMonitoring)
        {
            return;
        }

        IsMonitoringEnabled = true;
    }

    /// <summary>
    /// Кнопка загрузки и поле количества задач доступны.
    /// </summary>
    public bool CanFetchLatestIssues => !IsFetchIssuesBusy;

    /// <summary>
    /// Идёт запрос задач (смена лимита или ручная загрузка).
    /// </summary>
    public bool IsFetchIssuesBusy
    {
        get => _isFetchIssuesBusy;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isFetchIssuesBusy, value);
            this.RaisePropertyChanged(nameof(CanFetchLatestIssues));
        }
    }

    /// <summary>
    /// Текст индикатора загрузки задач.
    /// </summary>
    public string FetchingIssuesLabel =>
        _localization.GetString(UiStringKeys.Redmine.LabelFetchingIssues);

    /// <summary>
    /// Сколько задач запрашивать при загрузке и фоновом мониторинге (1–50).
    /// </summary>
    public decimal IssueFetchLimit
    {
        get => _issueFetchLimit;
        set
        {
            var clamped = Math.Clamp(value, (decimal)MinIssueFetchLimit, (decimal)MaxIssueFetchLimit);
            if (_issueFetchLimit == clamped)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _issueFetchLimit, clamped);
            if (!_suppressIssueFetchLimitSync)
            {
                ScheduleIssueFetchLimitSync();
            }
        }
    }

    /// <summary>
    /// Немедленно синхронизирует лимит задач с сервером (при уходе фокуса с поля ввода).
    /// </summary>
    public void CommitIssueFetchLimit()
    {
        if (_suppressIssueFetchLimitSync)
        {
            return;
        }

        CancelIssueFetchLimitDebounce();
        _ = ApplyIssueFetchLimitAsync((int)IssueFetchLimit);
    }

    /// <summary>
    /// Ключ статуса мониторинга Redmine.
    /// </summary>
    public string StatusKey
    {
        get => _statusKey;
        private set => this.RaiseAndSetIfChanged(ref _statusKey, value);
    }

    /// <summary>
    /// Дополнительный текст статуса (URL, фильтры, ошибка).
    /// </summary>
    public string? StatusDetail
    {
        get => _statusDetail;
        private set => this.RaiseAndSetIfChanged(ref _statusDetail, value);
    }

    /// <summary>
    /// Заголовок секции последнего события.
    /// </summary>
    public string LastEventSectionTitle =>
        _localization.GetString(UiStringKeys.Redmine.LabelLastEvent);

    /// <summary>
    /// Текст при отсутствии событий.
    /// </summary>
    public string NoLastEventDetail =>
        _localization.GetString(UiStringKeys.Redmine.DetailNoLastEvent);

    /// <summary>Строки таблицы последних задач.</summary>
    public ObservableCollection<RedmineIssueRowViewModel> IssueRows { get; }

    private async Task FetchLatestIssuesAsync()
    {
        IsFetchIssuesBusy = true;
        try
        {
            var token = await _ensureTokenAsync(CancellationToken.None).ConfigureAwait(false);
            var issues = await _redmineApiClient
                .GetLatestIssuesAsync(token, CancellationToken.None, (int)IssueFetchLimit)
                .ConfigureAwait(false);

            UpdateUi(() =>
            {
                ReplaceSourceIssues(issues.Select(issue => RedmineIssueRowViewModel.FromIssue(issue)));
            });
        }
        catch
        {
            UpdateUi(() => ReplaceSourceIssues([]));
        }
        finally
        {
            UpdateUi(() => IsFetchIssuesBusy = false);
        }
    }

    private async Task PollLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var token = await _ensureTokenAsync(cancellationToken).ConfigureAwait(false);
                await PollStatusAsync(token, cancellationToken).ConfigureAwait(false);
                await PollEventsAsync(token, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                UpdateUi(() =>
                {
                    StatusKey = UiStringKeys.Redmine.StatusDisconnected;
                    StatusDetail = _localization.GetString(UiStringKeys.Redmine.DetailServerUnavailable);
                });
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PollStatusAsync(string token, CancellationToken cancellationToken)
    {
        var status = await _redmineApiClient.GetStatusAsync(token, cancellationToken).ConfigureAwait(false);
        if (status is null)
        {
            return;
        }

        UpdateUi(() =>
        {
            SyncMonitoringEnabled(status.Enabled);
            SyncIssueFetchLimit(status.IssueFetchLimit);
            ApplyStatus(status);
        });
    }

    private async Task ApplyMonitoringToggleAsync(bool enabled)
    {
        var previous = !enabled;
        IsToggleBusy = true;
        try
        {
            var token = await _ensureTokenAsync(CancellationToken.None).ConfigureAwait(false);
            await _redmineApiClient
                .SetIssueFetchLimitAsync((int)IssueFetchLimit, token, CancellationToken.None)
                .ConfigureAwait(false);
            var status = await _redmineApiClient
                .SetEnabledAsync(enabled, token, CancellationToken.None)
                .ConfigureAwait(false);
            if (status is not null)
            {
                await RunOnUiThreadAsync(() =>
                {
                    SyncMonitoringEnabled(status.Enabled);
                    SyncIssueFetchLimit(status.IssueFetchLimit);
                    ApplyStatus(status);
                }).ConfigureAwait(false);

                if (status.Enabled)
                {
                    await ReloadMonitoredIssuesAsync(token, status.IssueFetchLimit).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            UpdateUi(() =>
            {
                SyncMonitoringEnabled(previous);
                StatusKey = UiStringKeys.Redmine.StatusDisconnected;
                StatusDetail = _localization.GetString(UiStringKeys.Redmine.DetailToggleFailed);
            });
        }
        finally
        {
            UpdateUi(() => IsToggleBusy = false);
        }
    }

    private void SyncMonitoringEnabled(bool enabled)
    {
        if (_isMonitoringEnabled == enabled)
        {
            return;
        }

        _suppressMonitoringSync = true;
        this.RaiseAndSetIfChanged(ref _isMonitoringEnabled, enabled);
        _suppressMonitoringSync = false;
    }

    private async Task ApplyIssueFetchLimitAsync(int limit)
    {
        if (limit == _syncedIssueFetchLimit)
        {
            return;
        }

        _issueFetchLimitApplyCts?.Cancel();
        _issueFetchLimitApplyCts?.Dispose();
        _issueFetchLimitApplyCts = CancellationTokenSource.CreateLinkedTokenSource(_disposeCts.Token);
        var applyToken = _issueFetchLimitApplyCts.Token;

        IsFetchIssuesBusy = true;
        try
        {
            var token = await _ensureTokenAsync(applyToken).ConfigureAwait(false);

            var setLimitTask = _redmineApiClient.SetIssueFetchLimitAsync(limit, token, applyToken);
            var issuesTask = _redmineApiClient.GetLatestIssuesAsync(
                token,
                applyToken,
                limit,
                syncSeenState: true);

            var issues = await issuesTask.ConfigureAwait(false);

            if (applyToken.IsCancellationRequested)
            {
                return;
            }

            await RunOnUiThreadAsync(() =>
            {
                ReplaceSourceIssues(issues.Select(issue => RedmineIssueRowViewModel.FromIssue(issue)));
            }).ConfigureAwait(false);

            var result = await setLimitTask.ConfigureAwait(false);
            if (result?.Status is null)
            {
                return;
            }

            await RunOnUiThreadAsync(() =>
            {
                SyncIssueFetchLimit(result.Status.IssueFetchLimit);
                ApplyStatus(result.Status);
            }).ConfigureAwait(false);

            _syncedIssueFetchLimit = result.Status.IssueFetchLimit;
        }
        catch (OperationCanceledException) when (applyToken.IsCancellationRequested)
        {
            // Заменён более новым запросом смены лимита.
        }
        catch
        {
            // Ошибка синхронизации лимита не блокирует работу вкладки.
        }
        finally
        {
            if (!applyToken.IsCancellationRequested)
            {
                await RunOnUiThreadAsync(() => IsFetchIssuesBusy = false).ConfigureAwait(false);
            }
        }
    }

    private void ScheduleIssueFetchLimitSync()
    {
        CancelIssueFetchLimitDebounce();
        _issueFetchLimitDebounceCts = CancellationTokenSource.CreateLinkedTokenSource(_disposeCts.Token);
        var debounceToken = _issueFetchLimitDebounceCts.Token;
        _ = RunIssueFetchLimitDebounceAsync(debounceToken);
    }

    private async Task RunIssueFetchLimitDebounceAsync(CancellationToken debounceToken)
    {
        try
        {
            await Task.Delay(IssueFetchLimitDebounce, debounceToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await ApplyIssueFetchLimitAsync((int)IssueFetchLimit).ConfigureAwait(false);
    }

    private void CancelIssueFetchLimitDebounce()
    {
        _issueFetchLimitDebounceCts?.Cancel();
        _issueFetchLimitDebounceCts?.Dispose();
        _issueFetchLimitDebounceCts = null;
    }

    private async Task ReloadMonitoredIssuesAsync(string token, int limit)
    {
        try
        {
            var issues = await _redmineApiClient
                .GetLatestIssuesAsync(token, CancellationToken.None, limit, syncSeenState: true)
                .ConfigureAwait(false);

            await RunOnUiThreadAsync(() =>
            {
                ReplaceSourceIssues(issues.Select(issue => RedmineIssueRowViewModel.FromIssue(issue)));
            }).ConfigureAwait(false);
        }
        catch
        {
            // Таблица остаётся в прежнем состоянии до следующей загрузки.
        }
    }

    private void SyncIssueFetchLimit(int limit)
    {
        if (limit <= 0)
        {
            return;
        }

        var clamped = Math.Clamp(limit, MinIssueFetchLimit, MaxIssueFetchLimit);
        _syncedIssueFetchLimit = clamped;
        if (_issueFetchLimit == (decimal)clamped)
        {
            return;
        }

        _suppressIssueFetchLimitSync = true;
        this.RaiseAndSetIfChanged(ref _issueFetchLimit, (decimal)clamped);
        _suppressIssueFetchLimitSync = false;
    }

    private async Task PollEventsAsync(string token, CancellationToken cancellationToken)
    {
        var events = await _redmineApiClient
            .GetEventsSinceAsync(_lastSeenEventUtc, token, cancellationToken)
            .ConfigureAwait(false);

        if (events.Count == 0)
        {
            return;
        }

        foreach (var payload in events.OrderBy(x => x.ReceivedAtUtc))
        {
            if (payload.ReceivedAtUtc > _lastSeenEventUtc)
            {
                _lastSeenEventUtc = payload.ReceivedAtUtc;
            }
        }

        var latestEvent = events.MaxBy(payload => payload.ReceivedAtUtc);
        if (latestEvent is null)
        {
            return;
        }

        await RunOnUiThreadAsync(() =>
        {
            foreach (var payload in events.OrderBy(x => x.ReceivedAtUtc))
            {
                UpsertIssueFromEvent(payload);
            }

            UpdateLastEvent(latestEvent);
        }).ConfigureAwait(false);
    }

    private void ApplyStatus(RedmineStatusPayload status)
    {
        if (!status.Enabled)
        {
            StatusKey = UiStringKeys.Redmine.StatusDisabled;
            StatusDetail = _localization.GetString(UiStringKeys.Redmine.DetailMonitoringOff);
            return;
        }

        if (status.Connected)
        {
            StatusKey = UiStringKeys.Redmine.StatusConnected;
            StatusDetail = BuildMonitoringDetail(status);
            return;
        }

        StatusKey = UiStringKeys.Redmine.StatusDisconnected;
        StatusDetail = BuildWaitingDetail(status);
    }

    private string BuildMonitoringDetail(RedmineStatusPayload status)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(status.BaseUrl))
        {
            parts.Add(status.BaseUrl);
        }

        if (!string.IsNullOrWhiteSpace(status.FiltersDescription))
        {
            parts.Add(status.FiltersDescription);
        }

        parts.Add(_localization.GetString(
            UiStringKeys.Redmine.DetailTrackedIssuesFormat,
            status.TrackedIssuesCount));

        return string.Join(" | ", parts);
    }

    private string BuildWaitingDetail(RedmineStatusPayload status)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(status.LastError))
        {
            parts.Add(status.LastError);
        }
        else
        {
            parts.Add(_localization.GetString(UiStringKeys.Redmine.DetailWaitingForPoll));
        }

        if (status.TrackedIssuesCount > 0)
        {
            parts.Add(_localization.GetString(
                UiStringKeys.Redmine.DetailTrackedIssuesFormat,
                status.TrackedIssuesCount));
        }

        return string.Join(" | ", parts);
    }

    private void UpdateLastEvent(RedmineEventPayload payload)
    {
        _lastEvent.UpdateFrom(payload);
        this.RaisePropertyChanged(nameof(HasLastEvent));
        this.RaisePropertyChanged(nameof(ShowNoLastEvent));
    }

    private void UpsertIssueFromEvent(RedmineEventPayload payload)
    {
        var existing = _sourceIssueRows.FirstOrDefault(row => row.Id == payload.IssueId);
        if (existing is not null)
        {
            existing.ApplyFromEvent(payload);
            _sourceIssueRows.Remove(existing);
            _sourceIssueRows.Insert(0, existing);
        }
        else
        {
            _sourceIssueRows.Insert(0, RedmineIssueRowViewModel.FromEvent(payload));
        }

        ApplyIssueFilters();
        RebuildFilterOptions();
    }

    /// <summary>
    /// Обновляет пункты меню фильтров перед открытием.
    /// </summary>
    public void RebuildFilterOptions()
    {
        RebuildFilterOptions(
            StatusFilterOptions,
            _sourceIssueRows.Select(row => row.StatusName),
            _issueTableFilter.Status);
        RebuildFilterOptions(
            PriorityFilterOptions,
            _sourceIssueRows.Select(row => row.PriorityName),
            _issueTableFilter.Priority);
        RebuildFilterOptions(
            VersionFilterOptions,
            _sourceIssueRows.Select(row => row.VersionName),
            _issueTableFilter.Version);
    }

    private void ReplaceSourceIssues(IEnumerable<RedmineIssueRowViewModel> rows)
    {
        _sourceIssueRows.Clear();
        _sourceIssueRows.AddRange(rows);
        ApplyIssueFilters();
        RebuildFilterOptions();
    }

    private void SetStatusFilter(string? value)
    {
        _issueTableFilter.Status = value;
        this.RaisePropertyChanged(nameof(IsStatusFilterActive));
        ApplyIssueFilters();
        RebuildFilterOptions();
    }

    private void SetPriorityFilter(string? value)
    {
        _issueTableFilter.Priority = value;
        this.RaisePropertyChanged(nameof(IsPriorityFilterActive));
        ApplyIssueFilters();
        RebuildFilterOptions();
    }

    private void SetVersionFilter(string? value)
    {
        _issueTableFilter.Version = value;
        this.RaisePropertyChanged(nameof(IsVersionFilterActive));
        ApplyIssueFilters();
        RebuildFilterOptions();
    }

    private void ApplyIssueFilters()
    {
        IssueRows.Clear();
        foreach (var row in _sourceIssueRows.Where(_issueTableFilter.Matches))
        {
            IssueRows.Add(row);
        }
    }

    private void RebuildFilterOptions(
        ObservableCollection<RedmineIssueFilterOptionViewModel> target,
        IEnumerable<string> values,
        string? selectedValue)
    {
        target.Clear();
        var allLabel = _localization.GetString(UiStringKeys.Redmine.FilterAll);
        target.Add(new RedmineIssueFilterOptionViewModel(allLabel, null, selectedValue is null));

        foreach (var value in values
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(value => value, StringComparer.Ordinal))
        {
            target.Add(new RedmineIssueFilterOptionViewModel(value, value, selectedValue == value));
        }
    }

    private void OnLocalizationCultureChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(LastEventSectionTitle));
        this.RaisePropertyChanged(nameof(NoLastEventDetail));
        this.RaisePropertyChanged(nameof(ShowNoLastEvent));
        this.RaisePropertyChanged(nameof(FetchingIssuesLabel));
        RebuildFilterOptions();
    }

    private static void UpdateUi(Action updateAction)
    {
        Dispatcher.UIThread.Post(updateAction);
    }

    private static async Task RunOnUiThreadAsync(Action updateAction)
    {
        await Dispatcher.UIThread.InvokeAsync(updateAction);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CancelIssueFetchLimitDebounce();
        _issueFetchLimitApplyCts?.Cancel();
        _issueFetchLimitApplyCts?.Dispose();
        _disposeCts.Cancel();
        _disposeCts.Dispose();
        _localization.CultureChanged -= OnLocalizationCultureChanged;
    }
}
