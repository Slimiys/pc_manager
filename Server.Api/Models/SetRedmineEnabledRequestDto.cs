namespace Server.Api.Models;

/// <summary>
/// Запрос на изменение состояния мониторинга Redmine.
/// </summary>
/// <param name="Enabled">Включить или выключить опрос.</param>
public sealed record SetRedmineEnabledRequestDto(bool Enabled);
