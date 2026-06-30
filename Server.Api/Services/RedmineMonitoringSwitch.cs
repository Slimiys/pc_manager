using Server.Application.Contracts;
using Server.Application.Redmine;

namespace Server.Api.Services;

/// <summary>
/// Потокобезопасное runtime-состояние включения мониторинга Redmine.
/// </summary>
public sealed class RedmineMonitoringSwitch : IRedmineMonitoringSwitch
{
    private int _enabled;

    /// <summary>
    /// Создаёт переключатель с начальным значением из конфигурации.
    /// </summary>
    /// <param name="options">Настройки Redmine.</param>
    public RedmineMonitoringSwitch(RedmineOptions options)
    {
        _enabled = options.Enabled ? 1 : 0;
    }

    /// <inheritdoc />
    public bool IsEnabled => Volatile.Read(ref _enabled) == 1;

    /// <inheritdoc />
    public void SetEnabled(bool enabled)
    {
        Volatile.Write(ref _enabled, enabled ? 1 : 0);
    }
}
