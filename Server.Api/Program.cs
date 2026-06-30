using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Server.Api.Services;
using Server.Application.Contracts;
using Server.Application.DependencyInjection;
using Server.Application.Redmine;
using Server.Infrastructure.Configuration;
using Server.Infrastructure.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
{
    if (eventArgs.ExceptionObject is Exception exception)
    {
        Log.Fatal(exception, "Unhandled domain exception in Server.Api.");
    }
};

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
{
    Log.Error(eventArgs.Exception, "Unobserved task exception in Server.Api.");
    eventArgs.SetObserved();
};

var dotEnvResult = DotEnvLoader.TryApplyRedmineFromFile(AppContext.BaseDirectory);
if (dotEnvResult.Loaded)
{
    Log.Information(
        "Файл .env загружен: {EnvPath}; применено переменных Redmine: {Count}",
        dotEnvResult.FilePath,
        dotEnvResult.AppliedRedmineKeys);
}
else
{
    Log.Information(
        "Файл .env не найден — настройки Redmine берутся из appsettings и переменных окружения.");
}

var builder = WebApplication.CreateBuilder(args);
if (TryGetPort(args, out var serverPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{serverPort}");
}
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<NotificationInMemoryStore>();
builder.Services.AddSingleton<INotificationBroadcaster, NotificationBroadcaster>();
builder.Services.AddSingleton<RedmineEventStore>();
builder.Services.AddSingleton<RedmineRuntimeStatus>();
builder.Services.AddSingleton<IRedmineMonitoringSwitch, RedmineMonitoringSwitch>();
builder.Services.AddSingleton<IRedmineIssueFetchLimit, RedmineIssueFetchLimit>();
builder.Services.AddSingleton<IRedminePollRunner, RedminePollRunner>();
builder.Services.AddSingleton<RedmineSeenStateSync>();
builder.Services.AddHostedService<LanDiscoveryHostedService>();
builder.Services.AddHostedService<RedminePollingHostedService>();

builder.Services.AddSingleton<IRedmineSeenStateStore>(sp =>
{
    var options = sp.GetRequiredService<Server.Application.Redmine.RedmineOptions>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = options.ResolveStateFilePath(env.ContentRootPath);
    return new Server.Infrastructure.Redmine.FileRedmineSeenStateStore(path);
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", configure =>
    {
        configure.Window = TimeSpan.FromSeconds(10);
        configure.PermitLimit = 20;
        configure.QueueLimit = 0;
    });
});

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "CHANGE_ME_FOR_PRODUCTION_32+_CHARS_SECRET";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // Токен в query access_token нужен для некоторых транспортов SignalR (браузер и прокси).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

// Webhook inbound проверяет только X-Notification-Key. Если в клиенте (Apidog) глобально включён
// Bearer с просроченным/пустым токеном, JWT middleware вернёт 401 до контроллера — убираем Authorization.
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/api/notifications/inbound"))
    {
        context.Request.Headers.Remove("Authorization");
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

app.MapControllers().RequireRateLimiting("api");
app.MapHub<Server.Api.Hubs.CommandEventsHub>("/hubs/events");

var inboundNotificationKey = app.Configuration["Notifications:InboundApiKey"];
if (string.IsNullOrWhiteSpace(inboundNotificationKey))
{
    Log.Warning("Notifications:InboundApiKey не задан — POST /api/notifications/inbound вернёт 503.");
}
else
{
    Log.Information(
        "Входящие HTTP-оповещения: для Apidog/webhook используйте заголовок X-Notification-Key: {InboundApiKey}",
        inboundNotificationKey);
}

app.Run();
Log.CloseAndFlush();

static bool TryGetPort(string[] args, out int port)
{
    port = 0;
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg.Equals("--port", StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedPort))
            {
                if (parsedPort is >= 1 and <= 65535)
                {
                    port = parsedPort;
                    return true;
                }
            }

            return false;
        }

        if (arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase))
        {
            var value = arg.Split('=', 2)[1];
            if (int.TryParse(value, out var parsedPort) && parsedPort is >= 1 and <= 65535)
            {
                port = parsedPort;
                return true;
            }

            return false;
        }
    }

    return false;
}

/// <summary>
/// Точка входа приложения для интеграционных тестов.
/// </summary>
public partial class Program;
