using Server.Application.Contracts;
using Server.Application.Redmine;

namespace Server.Api.Services;

/// <summary>
/// Потокобезопасное runtime-значение лимита задач Redmine за запрос.
/// </summary>
public sealed class RedmineIssueFetchLimit : IRedmineIssueFetchLimit
{
    private const int MinLimit = 1;
    private const int MaxLimit = 50;

    private int _value;

    /// <summary>
    /// Создаёт хранилище лимита с начальным значением из конфигурации.
    /// </summary>
    /// <param name="options">Настройки Redmine.</param>
    public RedmineIssueFetchLimit(RedmineOptions options)
    {
        _value = Clamp(options.IssueFetchLimit);
    }

    /// <inheritdoc />
    public int Value => Volatile.Read(ref _value);

    /// <inheritdoc />
    public void SetValue(int limit)
    {
        Volatile.Write(ref _value, Clamp(limit));
    }

    private static int Clamp(int limit) => Math.Clamp(limit, MinLimit, MaxLimit);
}
