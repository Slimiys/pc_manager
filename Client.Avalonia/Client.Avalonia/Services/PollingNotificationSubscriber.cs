using System.Globalization;
using Avalonia.Threading;
using Client.Avalonia.Localization;

namespace Client.Avalonia.Services;

/// <summary>
/// Периодический опрос API на предмет новых оповещений и вывод строк в историю UI.
/// </summary>
public sealed class PollingNotificationSubscriber : IDisposable
{
    private readonly NotificationsApiClient _notificationsApiClient;
    private readonly Func<CancellationToken, Task<string>> _getTokenAsync;
    private readonly ILocalizationService _localization;
    private readonly IToastService _toastService;
    private readonly Action<string> _addHistoryEntry;
    private readonly CancellationTokenSource _disposeCts = new();

    private DateTimeOffset _lastSeenUtc;

    /// <summary>
    /// Создаёт подписчик оповещений по HTTP-опросу.
    /// </summary>
    /// <param name="notificationsApiClient">Клиент API оповещений.</param>
    /// <param name="getTokenAsync">Получение JWT.</param>
    /// <param name="localization">Локализация строк истории.</param>
    /// <param name="toastService">Всплывающие уведомления.</param>
    /// <param name="addHistoryEntry">Добавление строки в историю (вызывается с UI-потока).</param>
    public PollingNotificationSubscriber(
        NotificationsApiClient notificationsApiClient,
        Func<CancellationToken, Task<string>> getTokenAsync,
        ILocalizationService localization,
        IToastService toastService,
        Action<string> addHistoryEntry)
    {
        _notificationsApiClient = notificationsApiClient;
        _getTokenAsync = getTokenAsync;
        _localization = localization;
        _toastService = toastService;
        _addHistoryEntry = addHistoryEntry;
        _lastSeenUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Запускает фоновый цикл опроса.
    /// </summary>
    public void Start()
    {
        _ = PollLoopAsync(_disposeCts.Token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposeCts.Cancel();
        _disposeCts.Dispose();
    }

    private async Task PollLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var token = await _getTokenAsync(cancellationToken).ConfigureAwait(false);
                var items = await _notificationsApiClient
                    .GetRecentSinceAsync(_lastSeenUtc, token, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var payload in items.OrderBy(x => x.ReceivedAtUtc))
                {
                    AppendOne(payload);
                    if (payload.ReceivedAtUtc > _lastSeenUtc)
                    {
                        _lastSeenUtc = payload.ReceivedAtUtc;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Сеть/API временно недоступны — повторим позже.
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

    private void AppendOne(NotificationBroadcastPayload payload)
    {
        var time = payload.ReceivedAtUtc.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var text = string.IsNullOrWhiteSpace(payload.Message)
            ? payload.Title
            : $"{payload.Title} — {payload.Message}";
        var line = _localization.GetString(UiStringKeys.History.HistoryNotificationFormat, time, text);
        Dispatcher.UIThread.Post(() =>
        {
            _addHistoryEntry(line);
            _toastService.ShowInformation(payload.Title, payload.Message);
        });
    }
}
