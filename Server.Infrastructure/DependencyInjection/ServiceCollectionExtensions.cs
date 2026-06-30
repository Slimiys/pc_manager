using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

using Server.Application.Contracts;

using Server.Infrastructure.Allowlist;

using Server.Infrastructure.Audit;

using Server.Infrastructure.Execution;

using Server.Infrastructure.Messaging;

using Server.Infrastructure.Messaging.Kafka;

using Server.Infrastructure.Redmine;

using Server.Infrastructure.Storage;

using Server.Infrastructure.System;



namespace Server.Infrastructure.DependencyInjection;



/// <summary>

/// Расширения регистрации инфраструктурных зависимостей.

/// </summary>

public static class ServiceCollectionExtensions

{

    /// <summary>

    /// Регистрирует инфраструктуру приложения.

    /// </summary>

    /// <param name="services">Коллекция сервисов.</param>

    /// <param name="configuration">Конфигурация.</param>

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)

    {

        services.AddSingleton<ICommandAllowlist, DefaultCommandAllowlist>();

        services.AddSingleton<ICommandRepository, InMemoryCommandRepository>();

        services.AddSingleton<IWorkstationLocker, WindowsWorkstationLocker>();

        services.AddSingleton<IAuditLogger, ConsoleAuditLogger>();

        services.AddSingleton<ISystemInfoProvider, DefaultSystemInfoProvider>();



        var agentOptions = configuration.GetSection("Agent").Get<AgentOptions>() ?? new AgentOptions();

        if (!string.IsNullOrWhiteSpace(agentOptions.BaseUrl))

        {

            services.AddSingleton(agentOptions);

            services.AddHttpClient<AgentCommandExecutor>(client => client.BaseAddress = new Uri(agentOptions.BaseUrl));

            services.AddTransient<ICommandExecutor>(provider => provider.GetRequiredService<AgentCommandExecutor>());

        }

        else

        {

            services.AddSingleton<ICommandExecutor, LocalCommandExecutor>();

        }



        var toastListenerBaseUrl = configuration["Notifications:ToastListenerBaseUrl"];

        if (!string.IsNullOrWhiteSpace(toastListenerBaseUrl))

        {

            // Обязательный завершающий / — иначе HttpClient склеивает путь с BaseAddress неверно.
            var toastBaseNormalized = toastListenerBaseUrl.TrimEnd('/') + "/";

            services.AddHttpClient<ToastListenerForwarder>(client => client.BaseAddress = new Uri(toastBaseNormalized));

            services.AddTransient<IToastListenerForwarder>(sp => sp.GetRequiredService<ToastListenerForwarder>());

        }

        else

        {

            services.AddSingleton<IToastListenerForwarder, NoOpToastListenerForwarder>();

        }



        var options = configuration.GetSection("Kafka").Get<KafkaOptions>() ?? new KafkaOptions();

        if (options.Enabled && !string.IsNullOrWhiteSpace(options.BootstrapServers))

        {

            services.AddSingleton(options);

            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        }

        else

        {

            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();

        }



        var redmineOptions = configuration.GetSection("Redmine").Get<Server.Application.Redmine.RedmineOptions>()
            ?? new Server.Application.Redmine.RedmineOptions();
        services.AddSingleton(redmineOptions);
        services.AddSingleton<Server.Application.Redmine.RedmineIssueMonitor>();
        services.AddHttpClient<RedmineApiClient>();
        services.AddSingleton<IRedmineApiClient>(provider => provider.GetRequiredService<RedmineApiClient>());



        return services;

    }

}


