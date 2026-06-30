using Client.Avalonia.Services;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// Строка таблицы задач Redmine.
/// </summary>
public sealed class RedmineIssueRowViewModel : ViewModelBase
{
    private bool _isNewHighlight;
    private string _subject;
    private string _statusName;
    private string _priorityName;
    private string _versionName;

    /// <summary>
    /// Создаёт строку таблицы из данных задачи.
    /// </summary>
    public RedmineIssueRowViewModel(
        int id,
        string subject,
        string statusName,
        string priorityName,
        string? versionName,
        DateTimeOffset? createdOnUtc = null,
        DateTimeOffset? updatedOnUtc = null,
        bool isNewHighlight = false)
    {
        Id = id;
        _subject = NormalizeText(subject);
        _statusName = NormalizeText(statusName);
        _priorityName = NormalizeText(priorityName);
        _versionName = NormalizeOptional(versionName);
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
        DateDisplay = RedmineIssueDateFormatter.Format(createdOnUtc, updatedOnUtc);
        _isNewHighlight = isNewHighlight;
    }

    /// <summary>Номер задачи.</summary>
    public int Id { get; }

    /// <summary>Тема задачи.</summary>
    public string Subject
    {
        get => _subject;
        private set => this.RaiseAndSetIfChanged(ref _subject, value);
    }

    /// <summary>Статус.</summary>
    public string StatusName
    {
        get => _statusName;
        private set => this.RaiseAndSetIfChanged(ref _statusName, value);
    }

    /// <summary>Приоритет.</summary>
    public string PriorityName
    {
        get => _priorityName;
        private set => this.RaiseAndSetIfChanged(ref _priorityName, value);
    }

    /// <summary>Версия.</summary>
    public string VersionName
    {
        get => _versionName;
        private set => this.RaiseAndSetIfChanged(ref _versionName, value);
    }

    /// <summary>Дата создания UTC (для сортировки).</summary>
    public DateTimeOffset? CreatedOnUtc { get; }

    /// <summary>Дата изменения UTC.</summary>
    public DateTimeOffset? UpdatedOnUtc { get; }

    /// <summary>Отформатированная дата для отображения.</summary>
    public string DateDisplay { get; }

    /// <summary>
    /// Подсветка новой или обновлённой задачи (оранжевый текст).
    /// </summary>
    public bool IsNewHighlight
    {
        get => _isNewHighlight;
        private set => this.RaiseAndSetIfChanged(ref _isNewHighlight, value);
    }

    /// <summary>
    /// Создаёт строку из ответа API задач.
    /// </summary>
    public static RedmineIssueRowViewModel FromIssue(RedmineIssuePayload issue, bool isNewHighlight = false) =>
        new(
            issue.Id,
            issue.Subject,
            issue.StatusName,
            issue.PriorityName,
            issue.VersionName,
            issue.CreatedOnUtc,
            issue.UpdatedOnUtc,
            isNewHighlight);

    /// <summary>
    /// Создаёт строку из события мониторинга.
    /// </summary>
    public static RedmineIssueRowViewModel FromEvent(RedmineEventPayload payload) =>
        new(
            payload.IssueId,
            payload.Subject,
            payload.StatusName,
            "—",
            payload.VersionName,
            payload.CreatedOnUtc == default ? null : payload.CreatedOnUtc,
            payload.UpdatedOnUtc == default ? null : payload.UpdatedOnUtc,
            isNewHighlight: true);

    /// <summary>
    /// Обновляет строку по событию мониторинга и подсвечивает её.
    /// </summary>
    public void ApplyFromEvent(RedmineEventPayload payload)
    {
        Subject = NormalizeText(payload.Subject);
        StatusName = NormalizeText(payload.StatusName);
        VersionName = NormalizeOptional(payload.VersionName);
        MarkAsNew();
    }

    /// <summary>
    /// Включает подсветку новой задачи.
    /// </summary>
    public void MarkAsNew()
    {
        IsNewHighlight = true;
    }

    /// <summary>
    /// Снимает подсветку (после наведения курсора).
    /// </summary>
    public void AcknowledgeHighlight()
    {
        if (!IsNewHighlight)
        {
            return;
        }

        IsNewHighlight = false;
    }

    private static string NormalizeText(string value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
}
