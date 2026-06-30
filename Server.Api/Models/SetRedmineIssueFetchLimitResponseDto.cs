namespace Server.Api.Models;

/// <summary>
/// Ответ на изменение лимита задач Redmine: статус и задачи из внепланового опроса.
/// </summary>
/// <param name="Status">Текущий статус мониторинга.</param>
/// <param name="Issues">Задачи, загруженные при опросе (пусто, если мониторинг выключен или опрос не удался).</param>
public sealed record SetRedmineIssueFetchLimitResponseDto(
    RedmineStatusDto Status,
    IReadOnlyList<RedmineIssueDto> Issues);
