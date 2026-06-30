using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Win32;
using Avalonia.X11;
using Client.Avalonia.Desktop.Services;
using Client.Avalonia.Services;
using Serilog;

namespace Client.Avalonia.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/ui-unhandled-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            if (eventArgs.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "Unhandled domain exception in Desktop UI.");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
        {
            Log.Error(eventArgs.Exception, "Unobserved task exception in Desktop UI.");
            eventArgs.SetObserved();
        };

        try
        {
            ToastNotificationCompatBootstrap.TryInitialize();
            ToastServiceFactory.Configure(static () => new WindowsSystemToastService());
            MainWindowTraySupport.Register();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Fatal startup exception in Desktop UI.");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(() => new X11PlatformOptions { OverlayPopups = true })
            .With(() => new Win32PlatformOptions { OverlayPopups = true })
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
