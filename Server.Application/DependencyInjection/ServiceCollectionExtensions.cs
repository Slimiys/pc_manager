using Microsoft.Extensions.DependencyInjection;
using Server.Application.Commands;

namespace Server.Application.DependencyInjection;

/// <summary>
/// Расширения регистрации application-слоя.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует сервисы application-слоя.
    /// </summary>
    /// <param name="services">Коллекция сервисов DI.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandService, CommandService>();
        return services;
    }
}
