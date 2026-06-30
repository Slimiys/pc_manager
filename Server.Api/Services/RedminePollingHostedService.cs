using Server.Application.Contracts;
using Server.Application.Redmine;

namespace Server.Api.Services;

/// <summary>
/// Фоновый опрос Redmine и публикация событий.
/// </summary>
public sealed class RedminePollingHostedService : BackgroundService
{
    private readonly IRedmineMonitoringSwitch _monitoringSwitch;
    private readonly IRedminePollRunner _pollRunner;
    private readonly RedmineOptions _options;
    private readonly RedmineRuntimeStatus _runtimeStatus;
    private readonly ILogger<RedminePollingHostedService> _logger;

    private string _lastLoggedError = string.Empty;
    private DateTimeOffset _lastLoggedErrorUtc = DateTimeOffset.MinValue;

    /// <summary>
    /// Создаёт фоновый сервис опроса Redmine.
    /// </summary>
    public RedminePollingHostedService(
        IRedmineMonitoringSwitch monitoringSwitch,
        IRedminePollRunner pollRunner,
        RedmineOptions options,
        RedmineRuntimeStatus runtimeStatus,
        ILogger<RedminePollingHostedService> logger)
    {
        _monitoringSwitch = monitoringSwitch;
        _pollRunner = pollRunner;
        _options = options;
        _runtimeStatus = runtimeStatus;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollInterval = TimeSpan.FromSeconds(Math.Max(5, _options.PollIntervalSeconds));
        var idleInterval = TimeSpan.FromSeconds(2);
        _logger.LogInformation(
            "Redmine polling service запущен (начальное состояние: {State}, URL: {BaseUrl}).",
            _monitoringSwitch.IsEnabled ? "включён" : "выключен",
            _options.BaseUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_monitoringSwitch.IsEnabled)
            {
                try
                {
                    await _pollRunner.RunAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogPollFailure(ex);
                }

                try
                {
                    await Task.Delay(pollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            else
            {
                try
                {
                    await Task.Delay(idleInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private void LogPollFailure(Exception ex)
    {
        var message = SanitizeLogMessage(ex.Message);
        var now = DateTimeOffset.UtcNow;
        if (string.Equals(message, _lastLoggedError, StringComparison.Ordinal)
            && now - _lastLoggedErrorUtc < TimeSpan.FromSeconds(60))
        {
            return;
        }

        _lastLoggedError = message;
        _lastLoggedErrorUtc = now;
        _logger.LogError(ex, "Redmine poll: {Message}", message);
        _runtimeStatus.UpdatePollResult(false, message, _runtimeStatus.GetSnapshot().TrackedIssuesCount);
    }

    private static string SanitizeLogMessage(string message) =>
        System.Text.RegularExpressions.Regex.Replace(message, "key=[^&\\s]+", "key=***");
}
