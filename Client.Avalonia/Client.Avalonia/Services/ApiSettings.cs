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
}
