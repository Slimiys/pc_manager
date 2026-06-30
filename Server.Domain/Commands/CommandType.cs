namespace Server.Domain.Commands;

/// <summary>
/// Тип поддерживаемой команды управления компьютером.
/// </summary>
public enum CommandType
{
    LockWorkstation = 1,
    GetUptime = 2,
    /// <summary>Схема питания Office (GUID как в telegram_shutdown_bot.py).</summary>
    SetPowerPlanOffice = 3,
    /// <summary>Схема питания Gaming.</summary>
    SetPowerPlanGaming = 4,
    /// <summary>Схема питания Performance.</summary>
    SetPowerPlanPerformance = 5
}
