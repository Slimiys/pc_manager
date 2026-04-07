using Server.Api.Models;

namespace Server.Api.Services;

/// <summary>
/// Потокобезопасное хранилище последних оповещений для выдачи клиентам по HTTP.
/// </summary>
public sealed class NotificationInMemoryStore
{
    private const int MaxItems = 500;

    private readonly object _sync = new();
    private readonly List<NotificationBroadcastDto> _items = new();

    /// <summary>
    /// Добавляет оповещение в хранилище.
    /// </summary>
    /// <param name="dto">Данные оповещения.</param>
    public void Add(NotificationBroadcastDto dto)
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
    /// Возвращает оповещения с временем строго позже указанной отметки (до лимита).
    /// </summary>
    /// <param name="sinceUtc">Нижняя граница по времени (UTC).</param>
    /// <param name="take">Максимум записей.</param>
    public IReadOnlyList<NotificationBroadcastDto> GetNewerThan(DateTimeOffset sinceUtc, int take = 100)
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
