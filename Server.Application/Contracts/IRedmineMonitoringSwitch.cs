namespace Server.Application.Contracts;

/// <summary>
/// Переключатель включения фонового мониторинга Redmine.
/// </summary>
public interface IRedmineMonitoringSwitch
{
    /// <summary>Признак активного мониторинга.</summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Включает или отключает мониторинг.
    /// </summary>
    /// <param name="enabled">Новое состояние.</param>
    void SetEnabled(bool enabled);
}
