using Client.Avalonia.Localization;

namespace Client.Avalonia.Services;

/// <summary>
/// Фоновый монитор доступности агента через периодический API-запрос.
/// </summary>
public sealed class AgentAvailabilityMonitor : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(3);

    private readonly CommandsApiClient _commandsApiClient;
    private readonly Func<CancellationToken, Task<string>> _getTokenAsync;
    private readonly Action<string> _onAvailabilityKeyChanged;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Создаёт монитор доступности агента.
    /// </summary>
    /// <param name="commandsApiClient">Клиент командного API.</param>
    /// <param name="getTokenAsync">Функция получения JWT с поддержкой отмены.</param>
    /// <param name="onAvailabilityKeyChanged">Коллбэк изменения ключа локализации статуса.</param>
    public AgentAvailabilityMonitor(
        CommandsApiClient commandsApiClient,
        Func<CancellationToken, Task<string>> getTokenAsync,
        Action<string> onAvailabilityKeyChanged)
    {
        _commandsApiClient = commandsApiClient;
        _getTokenAsync = getTokenAsync;
        _onAvailabilityKeyChanged = onAvailabilityKeyChanged;
    }

    /// <summary>
    /// Запускает фоновый цикл проверки доступности.
    /// </summary>
    public void Start()
    {
        _ = Task.Run(() => PollLoopAsync(_cts.Token));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private async Task PollLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var availabilityKey = UiStringKeys.Common.AgentAvailabilityOffline;
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(ProbeTimeout);

                var token = await _getTokenAsync(timeoutCts.Token).ConfigureAwait(false);
                var isAvailable = await _commandsApiClient
                    .IsAgentAvailableAsync(token, timeoutCts.Token)
                    .ConfigureAwait(false);
                availabilityKey = isAvailable
                    ? UiStringKeys.Common.AgentAvailabilityOnline
                    : UiStringKeys.Common.AgentAvailabilityOffline;
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
            catch
            {
                // Сеть/API временно недоступны — оставляем статус "Недоступен".
            }

            _onAvailabilityKeyChanged(availabilityKey);

            try
            {
                await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}

