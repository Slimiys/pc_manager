namespace Server.Application.Contracts;

/// <summary>
/// Текущий лимит задач Redmine за один запрос опроса.
/// </summary>
public interface IRedmineIssueFetchLimit
{
    /// <summary>Максимум задач в одном запросе (1–50).</summary>
    int Value { get; }

    /// <summary>
    /// Устанавливает лимит задач за запрос.
    /// </summary>
    /// <param name="limit">Новое значение; будет ограничено диапазоном 1–50.</param>
    void SetValue(int limit);
}
