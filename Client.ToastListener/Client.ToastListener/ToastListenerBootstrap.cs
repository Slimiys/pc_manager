using Client.ToastListener.Services;

namespace Client.ToastListener;

/// <summary>
/// Подключение HTTP-хоста после готовности окна и привязки toast.
/// </summary>
public static class ToastListenerBootstrap
{
    /// <summary>
    /// Вызывается после загрузки окна; задаётся из точки входа Desktop.
    /// </summary>
    public static Action<IToastService>? OnWindowReady { get; set; }

    /// <summary>
    /// Фабрика реализации <see cref="IToastService"/> (например системные toast Windows в Desktop).
    /// Если не задана, используется <see cref="WindowToastService"/> внутри окна.
    /// </summary>
    public static Func<IToastService>? CreateToastService { get; set; }
}
