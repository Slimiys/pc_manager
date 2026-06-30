namespace Server.Application.Redmine;

/// <summary>
/// Результат одного цикла опроса Redmine.
/// </summary>
/// <param name="Issues">Задачи из последнего ответа API.</param>
/// <param name="EventsCount">Число сгенерированных событий.</param>
public sealed record RedminePollResult(
    IReadOnlyList<RedmineIssue> Issues,
    int EventsCount);
