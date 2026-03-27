namespace Client.Avalonia.Services;

/// <summary>
/// Клиент получения токена авторизации.
/// </summary>
public sealed class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;

    /// <summary>
    /// Создает клиент авторизации.
    /// </summary>
    /// <param name="httpClient">HTTP клиент.</param>
    /// <param name="apiSettings">Настройки API.</param>
    public AuthApiClient(HttpClient httpClient, ApiSettings apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings;
    }

    /// <summary>
    /// Запрашивает новый JWT токен.
    /// </summary>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync(
            $"api/auth/token?role={Uri.EscapeDataString(_apiSettings.Role)}",
            content: null,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawToken = await response.Content.ReadAsStringAsync(cancellationToken);
        var token = rawToken.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("API returned empty token.");
        }

        return token;
    }
}
