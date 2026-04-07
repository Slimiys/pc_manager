namespace Server.Application.Contracts;

/// <summary>
/// Пересылка текста оповещения на десктоп-приложение (HTTP toast-слушатель).
/// </summary>
public interface IToastListenerForwarder
{
    /// <summary>
    /// Отправляет запрос на показ toast на машине со слушателем.
    /// </summary>
    /// <param name="title">Заголовок.</param>
    /// <param name="message">Дополнительный текст.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task ForwardAsync(string title, string? message, CancellationToken cancellationToken);
}
