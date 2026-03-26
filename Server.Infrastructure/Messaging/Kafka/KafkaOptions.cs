namespace Server.Infrastructure.Messaging.Kafka;

/// <summary>
/// Настройки подключения к Kafka.
/// </summary>
public sealed class KafkaOptions
{
    /// <summary>
    /// Флаг включения Kafka-публикации.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Адрес bootstrap серверов Kafka.
    /// </summary>
    public string BootstrapServers { get; init; } = string.Empty;
}
