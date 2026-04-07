using Avalonia.Controls;

namespace Client.Avalonia.Services;

/// <summary>
/// Показ всплывающих (toast) уведомлений поверх главного окна.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Привязывает менеджер уведомлений к корневому элементу (окно или TopLevel).
    /// </summary>
    /// <param name="topLevel">Корень визуального дерева.</param>
    void Attach(TopLevel topLevel);

    /// <summary>
    /// Показывает информационное уведомление с заголовком и текстом.
    /// </summary>
    /// <param name="title">Заголовок.</param>
    /// <param name="message">Текст; может быть null или пустым.</param>
    void ShowInformation(string title, string? message = null);
}
