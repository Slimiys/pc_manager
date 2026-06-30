using System.Globalization;

namespace Client.Avalonia.ViewModels;

/// <summary>
/// Форматирует даты создания и изменения задачи Redmine для таблицы.
/// </summary>
internal static class RedmineIssueDateFormatter
{
    /// <summary>
    /// Возвращает дату создания; при отличии даты изменения добавляет её в скобках.
    /// </summary>
    /// <param name="createdOnUtc">Дата создания UTC.</param>
    /// <param name="updatedOnUtc">Дата изменения UTC.</param>
    /// <param name="formatProvider">Формат даты; по умолчанию текущая культура.</param>
    public static string Format(
        DateTimeOffset? createdOnUtc,
        DateTimeOffset? updatedOnUtc,
        IFormatProvider? formatProvider = null)
    {
        if (createdOnUtc is null)
        {
            return "—";
        }

        var provider = formatProvider ?? CultureInfo.CurrentCulture;
        var createdLocal = createdOnUtc.Value.ToLocalTime();
        var createdText = createdLocal.ToString("d", provider);

        if (updatedOnUtc is null)
        {
            return createdText;
        }

        var updatedLocal = updatedOnUtc.Value.ToLocalTime();
        if (createdLocal.Date == updatedLocal.Date)
        {
            return createdText;
        }

        return $"{createdText} ({updatedLocal.ToString("d", provider)})";
    }
}
