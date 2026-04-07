using Avalonia.Controls;

namespace Client.ToastListener.Services;

/// <summary>
/// Показ всплывающих уведомлений поверх окна.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Привязывает менеджер уведомлений к корневому элементу.
    /// </summary>
    /// <param name="topLevel">Корень визуального дерева.</param>
    void Attach(TopLevel topLevel);

    /// <summary>
    /// Показывает информационное уведомление.
    /// </summary>
    /// <param name="title">Заголовок.</param>
    /// <param name="message">Текст; может быть null или пустым.</param>
    void ShowInformation(string title, string? message = null);
}
