namespace Server.Api.Models;

/// <summary>
/// Тело запроса изменения лимита задач Redmine.
/// </summary>
/// <param name="IssueFetchLimit">Максимум задач за один запрос (1–50).</param>
public sealed record SetRedmineIssueFetchLimitRequestDto(int IssueFetchLimit);
