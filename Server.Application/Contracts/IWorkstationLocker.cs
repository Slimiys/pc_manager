namespace Server.Application.Contracts;

/// <summary>
/// Сервис блокировки рабочей станции.
/// </summary>
public interface IWorkstationLocker
{
    /// <summary>
    /// Выполняет блокировку рабочей станции.
    /// </summary>
    void Lock();
}
