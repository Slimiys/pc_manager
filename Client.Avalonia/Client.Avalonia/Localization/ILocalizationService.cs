using System.Globalization;

namespace Client.Avalonia.Localization;

/// <summary>
/// Сервис локализации UI: строки по ключам и смена языка в рантайме.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Текущая культура интерфейса.
    /// </summary>
    CultureInfo CurrentCulture { get; set; }

    /// <summary>
    /// Событие смены языка (для обновления привязок).
    /// </summary>
    event EventHandler? CultureChanged;

    /// <summary>
    /// Возвращает локализованную строку по ключу ресурса.
    /// </summary>
    /// <param name="resourceKey">Ключ в RESX.</param>
    /// <returns>Строка или ключ, если перевод не найден.</returns>
    string GetString(string resourceKey);

    /// <summary>
    /// Возвращает локализованную строку с подстановкой аргументов (<see cref="string.Format(string, object?[])"/>).
    /// </summary>
    /// <param name="resourceKey">Ключ в RESX.</param>
    /// <param name="formatArgs">Аргументы форматирования.</param>
    /// <returns>Отформатированная строка.</returns>
    string GetString(string resourceKey, params object?[] formatArgs);
}
