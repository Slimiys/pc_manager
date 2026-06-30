using System.Globalization;
using Client.Avalonia.Services;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// Отображение последнего события Redmine.
/// </summary>
public sealed class RedmineLastEventViewModel : ViewModelBase
{
    private bool _hasEvent;
    private string _receivedAtDisplay = "—";
    private string _issueIdDisplay = "—";
    private string _versionDisplay = "—";
    private string _projectDisplay = "—";
    private string _subjectDisplay = "—";
    private string _dateDisplay = "—";
    private string _statusDisplay = "—";

    /// <summary>Признак наличия события для отображения.</summary>
    public bool HasEvent
    {
        get => _hasEvent;
        private set => this.RaiseAndSetIfChanged(ref _hasEvent, value);
    }

    /// <summary>Время получения события (локальное).</summary>
    public string ReceivedAtDisplay
    {
        get => _receivedAtDisplay;
        private set => this.RaiseAndSetIfChanged(ref _receivedAtDisplay, value);
    }

    /// <summary>Номер задачи.</summary>
    public string IssueIdDisplay
    {
        get => _issueIdDisplay;
        private set => this.RaiseAndSetIfChanged(ref _issueIdDisplay, value);
    }

    /// <summary>Версия задачи.</summary>
    public string VersionDisplay
    {
        get => _versionDisplay;
        private set => this.RaiseAndSetIfChanged(ref _versionDisplay, value);
    }

    /// <summary>Проект задачи.</summary>
    public string ProjectDisplay
    {
        get => _projectDisplay;
        private set => this.RaiseAndSetIfChanged(ref _projectDisplay, value);
    }

    /// <summary>Тема задачи.</summary>
    public string SubjectDisplay
    {
        get => _subjectDisplay;
        private set => this.RaiseAndSetIfChanged(ref _subjectDisplay, value);
    }

    /// <summary>Дата создания и при необходимости изменения.</summary>
    public string DateDisplay
    {
        get => _dateDisplay;
        private set => this.RaiseAndSetIfChanged(ref _dateDisplay, value);
    }

    /// <summary>Статус задачи.</summary>
    public string StatusDisplay
    {
        get => _statusDisplay;
        private set => this.RaiseAndSetIfChanged(ref _statusDisplay, value);
    }

    /// <summary>
    /// Обновляет модель по событию с сервера.
    /// </summary>
    /// <param name="payload">Событие Redmine.</param>
    public void UpdateFrom(RedmineEventPayload payload)
    {
        HasEvent = true;
        ReceivedAtDisplay = payload.ReceivedAtUtc
            .ToLocalTime()
            .ToString("G", CultureInfo.CurrentCulture);
        IssueIdDisplay = payload.IssueId.ToString(CultureInfo.InvariantCulture);
        VersionDisplay = NormalizeOptional(payload.VersionName);
        ProjectDisplay = NormalizeText(payload.ProjectName);
        SubjectDisplay = NormalizeText(payload.Subject);
        DateDisplay = RedmineIssueDateFormatter.Format(
            ToNullableUtc(payload.CreatedOnUtc),
            ToNullableUtc(payload.UpdatedOnUtc));
        StatusDisplay = NormalizeText(payload.StatusName);
    }

    /// <summary>Сбрасывает отображение.</summary>
    public void Clear()
    {
        HasEvent = false;
        ReceivedAtDisplay = "—";
        IssueIdDisplay = "—";
        VersionDisplay = "—";
        ProjectDisplay = "—";
        SubjectDisplay = "—";
        DateDisplay = "—";
        StatusDisplay = "—";
    }

    private static string NormalizeText(string value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static DateTimeOffset? ToNullableUtc(DateTimeOffset value) =>
        value == default ? null : value;
}
