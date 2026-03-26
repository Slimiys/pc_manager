using Confluent.Kafka;
using Server.Application.Contracts;

namespace Server.Infrastructure.Messaging.Kafka;

/// <summary>
/// Публикует события в Kafka топики.
/// </summary>
public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    /// <summary>
    /// Создает Kafka publisher.
    /// </summary>
    /// <param name="options">Настройки Kafka.</param>
    public KafkaEventPublisher(KafkaOptions options)
    {
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers
        }).Build();
    }

    /// <inheritdoc />
    public async Task PublishAsync(string topic, string payload, CancellationToken cancellationToken)
    {
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString("N"),
            Value = payload
        }, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(2));
        _producer.Dispose();
    }
}
