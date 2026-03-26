namespace Server.Application.Contracts;

/// <summary>
/// Предоставляет информацию о системе хоста.
/// </summary>
public interface ISystemInfoProvider
{
    /// <summary>
    /// Возвращает краткую информацию о системе.
    /// </summary>
    Task<SystemInfoDto> GetAsync();
}
