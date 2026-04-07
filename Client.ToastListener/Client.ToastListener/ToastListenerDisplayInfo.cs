namespace Client.ToastListener;

/// <summary>
/// Данные для подсказки в окне: ключ и адрес слушателя (заполняются из Desktop Program до показа UI).
/// </summary>
public static class ToastListenerDisplayInfo
{
    /// <summary>
    /// Ожидаемое значение заголовка X-Toast-Listener-Key (совпадает с Notifications:ToastListenerKey на сервере).
    /// </summary>
    public static string ExpectedApiKey { get; private set; } = string.Empty;

    /// <summary>
    /// Строка вида http://host:port для подсказки.
    /// </summary>
    public static string ListenAddress { get; private set; } = string.Empty;

    /// <summary>
    /// Заполняет поля из секции ToastListener.
    /// </summary>
    /// <param name="apiKey">ToastListener:ApiKey.</param>
    /// <param name="httpPort">ToastListener:HttpPort.</param>
    /// <param name="bindHost">ToastListener:BindHost; пусто — 0.0.0.0.</param>
    public static void Apply(string? apiKey, int httpPort, string? bindHost)
    {
        if (string.IsNullOrWhiteSpace(bindHost))
        {
            bindHost = "0.0.0.0";
        }

        ExpectedApiKey = apiKey ?? string.Empty;
        ListenAddress = $"http://{bindHost}:{httpPort}";
    }
}
