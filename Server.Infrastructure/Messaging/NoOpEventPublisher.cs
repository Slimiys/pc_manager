using Server.Application.Contracts;

namespace Server.Infrastructure.Messaging;

/// <summary>
/// Пустая реализация publisher для MVP без Kafka.
/// </summary>
public sealed class NoOpEventPublisher : IEventPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(string topic, string payload, CancellationToken cancellationToken) => Task.CompletedTask;
}
