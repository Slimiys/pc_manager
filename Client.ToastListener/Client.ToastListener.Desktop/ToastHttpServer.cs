using Client.ToastListener.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading;

namespace Client.ToastListener.Desktop;

/// <summary>
/// Встроенный Kestrel: приём HTTP-запросов на показ toast.
/// </summary>
public static class ToastHttpServer
{
    /// <summary>
    /// Запускает HTTP-слушатель до отмены или остановки приложения.
    /// </summary>
    /// <param name="toastService">Сервис показа уведомлений (маршрутизация на UI-поток).</param>
    /// <param name="configuration">Конфигурация (ключ и порт).</param>
    /// <param name="cancellationToken">Отмена при остановке хоста (служба Windows).</param>
    public static async Task RunAsync(
        IToastService toastService,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var port = configuration.GetValue("ToastListener:HttpPort", 8787);
        // 0.0.0.0 — чтобы запросы доходили с Docker (host.docker.internal) и с сети; только 127.0.0.1 режет такие вызовы.
        var bindHost = configuration["ToastListener:BindHost"];
        if (string.IsNullOrWhiteSpace(bindHost))
        {
            bindHost = "0.0.0.0";
        }

        var apiKey = configuration["ToastListener:ApiKey"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Укажите непустой ToastListener:ApiKey в appsettings.json или переменной окружения.");
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            Args = Array.Empty<string>()
        });

        builder.WebHost.UseSetting("urls", $"http://{bindHost}:{port}");

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");

        var app = builder.Build();

        // Тот же вывод, что и у Serilog в Program.cs (ILogger Kestrel после ClearProviders часто не виден в общей консоли).
        Log.Information(
            "Toast listener: ожидаемый заголовок X-Toast-Listener-Key = {ApiKey}; слушаем http://{BindHost}:{Port}/api/toast/show",
            apiKey,
            bindHost,
            port);

        app.MapPost(
            "/api/toast/show",
            (HttpRequest request, ToastShowDto dto, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("ToastHttp");
                if (!request.Headers.TryGetValue("X-Toast-Listener-Key", out var keyHeader))
                {
                    return Results.Unauthorized();
                }

                var providedKey = keyHeader.ToString().Trim();
                if (!string.Equals(providedKey, apiKey.Trim(), StringComparison.Ordinal))
                {
                    logger.LogWarning(
                        "Неверный X-Toast-Listener-Key (длина получена {Len}, ожидается {ExpectedLen})",
                        providedKey.Length,
                        apiKey.Length);
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return Results.BadRequest("Поле title обязательно.");
                }

                toastService.ShowInformation(dto.Title, dto.Message);
                logger.LogInformation(
                    "Запрос на показ принят: {Title} (ошибка отображения — в логе приложения, если есть).",
                    dto.Title);
                return Results.Ok();
            });

        await app.StartAsync(cancellationToken);
        await app.WaitForShutdownAsync(cancellationToken);
    }
}

/// <summary>
/// Тело запроса показа уведомления (JSON: title, message).
/// </summary>
/// <param name="Title">Заголовок.</param>
/// <param name="Message">Дополнительный текст.</param>
public sealed record ToastShowDto(string Title, string? Message);
