namespace Client.Avalonia.Services;

/// <summary>
/// Фабрика реализации <see cref="IToastService"/> для конкретной платформы.
/// </summary>
public static class ToastServiceFactory
{
    private static Func<IToastService>? _create;

    /// <summary>
    /// Задаёт фабрику toast-сервиса (вызывается из точки входа Desktop до старта Avalonia).
    /// </summary>
    /// <param name="create">Создание экземпляра сервиса.</param>
    public static void Configure(Func<IToastService> create)
    {
        _create = create;
    }

    /// <summary>
    /// Создаёт toast-сервис: платформенный или встроенный в окно по умолчанию.
    /// </summary>
    public static IToastService Create() => _create?.Invoke() ?? new WindowToastService();
}
