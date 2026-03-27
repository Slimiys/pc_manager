namespace Client.Avalonia.Services;

/// <summary>
/// Кэш JWT токена в рамках сессии приложения.
/// </summary>
public sealed class TokenCache
{
    private string? _token;

    /// <summary>
    /// Возвращает признак наличия токена.
    /// </summary>
    public bool HasToken => !string.IsNullOrWhiteSpace(_token);

    /// <summary>
    /// Возвращает текущий токен.
    /// </summary>
    public string? GetToken() => _token;

    /// <summary>
    /// Сохраняет токен.
    /// </summary>
    /// <param name="token">JWT токен.</param>
    public void SetToken(string token)
    {
        _token = token;
    }
}
