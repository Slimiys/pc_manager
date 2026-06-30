namespace Server.Application.Redmine;

/// <summary>
/// Режим цикла опроса Redmine.
/// </summary>
public sealed class RedminePollOptions
{
    /// <summary>Полный опрос: сравнение, события и уведомления.</summary>
    public static RedminePollOptions Monitoring { get; } = new(detectChanges: true);

    /// <summary>Загрузка задач и тихое обновление состояния без событий (смена лимита).</summary>
    public static RedminePollOptions LimitSync { get; } = new(detectChanges: false);

    private RedminePollOptions(bool detectChanges)
    {
        DetectChanges = detectChanges;
    }

    /// <summary>Нужно ли сравнивать с сохранённым состоянием и публиковать события.</summary>
    public bool DetectChanges { get; }
}
