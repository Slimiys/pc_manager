using Server.Api.Models;

namespace Server.Api.Services;

/// <summary>
/// Потокобезопасное хранилище событий Redmine для UI.
/// </summary>
public sealed class RedmineEventStore
{
    private const int MaxItems = 500;

    private readonly object _sync = new();
    private readonly List<RedmineEventDto> _items = new();

    /// <summary>
    /// Добавляет событие Redmine.
    /// </summary>
    /// <param name="dto">Данные события.</param>
    public void Add(RedmineEventDto dto)
    {
        lock (_sync)
        {
            _items.Add(dto);
            while (_items.Count > MaxItems)
            {
                _items.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Возвращает события после указанного времени.
    /// </summary>
    /// <param name="sinceUtc">Нижняя граница UTC.</param>
    /// <param name="take">Максимум записей.</param>
    public IReadOnlyList<RedmineEventDto> GetNewerThan(DateTimeOffset sinceUtc, int take = 100)
    {
        lock (_sync)
        {
            return _items
                .Where(x => x.ReceivedAtUtc > sinceUtc)
                .OrderBy(x => x.ReceivedAtUtc)
                .Take(take)
                .ToList();
        }
    }
}
