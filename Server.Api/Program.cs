using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Server.Application.DependencyInjection;
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
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

app.MapControllers().RequireRateLimiting("api");
app.MapHub<Server.Api.Hubs.CommandEventsHub>("/hubs/events");

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
