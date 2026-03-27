using Server.Application.Contracts;
using Server.Domain.Commands;
using Server.Infrastructure.Execution;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
{
    if (eventArgs.ExceptionObject is Exception exception)
    {
        Log.Fatal(exception, "Unhandled domain exception in Host.Agent.");
    }
};

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
{
    Log.Error(eventArgs.Exception, "Unobserved task exception in Host.Agent.");
    eventArgs.SetObserved();
};

var builder = WebApplication.CreateBuilder(args);
if (TryGetPort(args, out var agentPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{agentPort}");
}
builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IWorkstationLocker, WindowsWorkstationLocker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.MapPost("/api/agent/execute", (HttpRequest request, ExecuteAgentCommandDto dto, IWorkstationLocker workstationLocker) =>
{
    var expectedApiKey = app.Configuration["Agent:ApiKey"] ?? "CHANGE_ME_AGENT_KEY";
    if (!request.Headers.TryGetValue("X-Agent-Key", out var apiKey) || apiKey != expectedApiKey)
    {
        return Results.Unauthorized();
    }

    if (!Enum.TryParse<CommandType>(dto.Type, true, out var commandType))
    {
        return Results.BadRequest("Unknown command type.");
    }

    var result = commandType switch
    {
        CommandType.GetUptime => (DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64)).ToString(),
        CommandType.LockWorkstation =>
            LockWorkstationAndReturnResult(workstationLocker),
        _ => throw new InvalidOperationException($"Unsupported command type: {commandType}")
    };

    return Results.Ok(new ExecuteAgentCommandResultDto(result));
});

app.Run();
Log.CloseAndFlush();

static string LockWorkstationAndReturnResult(IWorkstationLocker workstationLocker)
{
    workstationLocker.Lock();
    return "Workstation lock requested successfully.";
}

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
/// DTO запроса команды для host-агента.
/// </summary>
/// <param name="Type">Тип команды.</param>
/// <param name="Payload">Параметры команды.</param>
public sealed record ExecuteAgentCommandDto(string Type, string? Payload);

/// <summary>
/// DTO результата выполнения команды.
/// </summary>
/// <param name="Result">Текстовый результат.</param>
public sealed record ExecuteAgentCommandResultDto(string Result);
