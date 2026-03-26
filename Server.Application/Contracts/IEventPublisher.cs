namespace Server.Application.Contracts;

/// <summary>
/// Публикует доменные события во внешнюю шину.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Публикует событие в заданный топик.
    /// </summary>
    /// <param name="topic">Имя топика.</param>
    /// <param name="payload">Сериализованное событие.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task PublishAsync(string topic, string payload, CancellationToken cancellationToken);
}
