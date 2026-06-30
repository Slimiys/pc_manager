using System.Diagnostics.CodeAnalysis;

namespace Server.Domain.Commands;

/// <summary>
/// GUID схем питания Windows, совпадающие с POWER_PLANS из telegram_shutdown_bot.py.
/// </summary>
public static class PowerPlanGuids
{
    /// <summary>GUID пользовательской схемы «Office».</summary>
    public const string Office = "5da85d85-eb0c-4710-a805-6da8bbcf1f02";

    /// <summary>GUID пользовательской схемы «Gaming».</summary>
    public const string Gaming = "653fbb7f-248b-41d0-aa49-03fabbfd5e8e";

    /// <summary>GUID пользовательской схемы «Performance».</summary>
    public const string Performance = "a9932169-f1fb-4dff-9034-fe1c4ca1886e";

    /// <summary>
    /// Возвращает GUID схемы для команды смены питания.
    /// </summary>
    /// <param name="commandType">Тип команды.</param>
    /// <param name="schemeGuid">GUID схемы при успехе.</param>
    /// <returns>True, если команда — смена схемы питания.</returns>
    public static bool TryGetForCommand(CommandType commandType, [NotNullWhen(true)] out string? schemeGuid)
    {
        switch (commandType)
        {
            case CommandType.SetPowerPlanOffice:
                schemeGuid = Office;
                return true;
            case CommandType.SetPowerPlanGaming:
                schemeGuid = Gaming;
                return true;
            case CommandType.SetPowerPlanPerformance:
                schemeGuid = Performance;
                return true;
            default:
                schemeGuid = null;
                return false;
        }
    }
}
