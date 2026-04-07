using Client.ToastListener.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Client.ToastListener.Desktop;

/// <summary>
/// Фоновый запуск Kestrel для приёма HTTP-запросов toast (режим Windows Service).
/// </summary>
internal sealed class ToastListenerHostedService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IToastService _toastService;

    /// <summary>
    /// Создаёт сервис с внедрёнными конфигурацией и реализацией toast.
    /// </summary>
    public ToastListenerHostedService(IConfiguration configuration, IToastService toastService)
    {
        _configuration = configuration;
        _toastService = toastService;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ToastHttpServer.RunAsync(_toastService, _configuration, stoppingToken);
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_toastService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
