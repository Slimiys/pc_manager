namespace Client.Avalonia.Services;

/// <summary>
/// Настройки API для UI-клиента.
/// </summary>
public sealed class ApiSettings
{
    /// <summary>
    /// Базовый адрес API.
    /// </summary>
    public string BaseUrl { get; init; } = "http://localhost:8080";

    /// <summary>
    /// Роль для запроса токена.
    /// </summary>
    public string Role { get; init; } = "Operator";

    /// <summary>
    /// Ключ для заголовка X-Notification-Key в endpoint api/notifications/inbound.
    /// </summary>
    public string InboundNotificationKey { get; init; } = "a7f3c9e2b84d1f6a0e5c8b3d9f2a4e6c1b8d0f5a3";

    /// <summary>
    /// Включает LAN-discovery API по UDP.
    /// </summary>
    public bool EnableLanDiscovery { get; init; } = true;

    /// <summary>
    /// Идентификатор пары устройств для LAN-discovery.
    /// </summary>
    public string DiscoveryPairId { get; init; } = "pc-manager-default";

    /// <summary>
    /// Общий ключ подписи HMAC для LAN-discovery.
    /// </summary>
    public string DiscoverySharedKey { get; init; } = "CHANGE_ME_DISCOVERY_SHARED_KEY";

    /// <summary>
    /// UDP-порт discovery.
    /// </summary>
    public int DiscoveryUdpPort { get; init; } = 37020;

    /// <summary>
    /// Таймаут поиска API в LAN в миллисекундах.
    /// </summary>
    public int DiscoveryTimeoutMs { get; init; } = 1800;
}
