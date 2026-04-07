using Client.ToastListener;
using Client.ToastListener.Desktop.Services;
using Client.ToastListener.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Runtime.InteropServices;

namespace Client.ToastListener.Desktop;

/// <summary>
/// Точка входа: консольный listener.
/// </summary>
internal static class Program
{
    private const string NoWindowArg = "-no-window";
    private const int SwHide = 0;

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void Main(string[] args)
    {
        if (args.Contains(NoWindowArg, StringComparer.OrdinalIgnoreCase))
        {
            HideConsoleWindow();
        }

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            if (eventArgs.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "Необработанное исключение в Toast Listener.");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
        {
            Log.Error(eventArgs.Exception, "Неотслеженное исключение задачи в Toast Listener.");
            eventArgs.SetObserved();
        };

        try
        {
            RunHost(args);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Критическая ошибка запуска Toast Listener.");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Скрывает консольное окно процесса.
    /// </summary>
    private static void HideConsoleWindow()
    {
        var handle = GetConsoleWindow();
        if (handle != IntPtr.Zero)
        {
            ShowWindow(handle, SwHide);
        }
    }

    /// <summary>
    /// Запускает generic host в консольном режиме.
    /// </summary>
    /// <param name="args">Аргументы процесса.</param>
    private static void RunHost(string[] args)
    {
        var hostArgs = args
            .Where(a => !string.Equals(a, NoWindowArg, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory,
            Args = hostArgs
        });

        builder.Services.AddSingleton<IToastService, StaWindowsToastService>();
        builder.Services.AddHostedService<ToastListenerHostedService>();

        var httpPort = builder.Configuration.GetValue("ToastListener:HttpPort", 8787);
        var bindHost = builder.Configuration["ToastListener:BindHost"];
        ToastListenerDisplayInfo.Apply(builder.Configuration["ToastListener:ApiKey"], httpPort, bindHost);
        Log.Information(
            "ToastListener (консоль): ожидаемый X-Toast-Listener-Key = {ApiKey}; {Listen}",
            ToastListenerDisplayInfo.ExpectedApiKey,
            ToastListenerDisplayInfo.ListenAddress);

        using var host = builder.Build();
        host.Run();
    }
}
