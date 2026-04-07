using Avalonia.Controls;

namespace Client.Avalonia.Services;

/// <summary>
/// Пустая реализация для конструкторов по умолчанию (дизайнер XAML).
/// </summary>
public sealed class NullToastService : IToastService
{
    /// <summary>
    /// Единственный экземпляр.
    /// </summary>
    public static readonly NullToastService Instance = new();

    private NullToastService()
    {
    }

    /// <inheritdoc />
    public void Attach(TopLevel topLevel)
    {
        // Ничего: не используется в рантайме.
    }

    /// <inheritdoc />
    public void ShowInformation(string title, string? message = null)
    {
        // Ничего: не используется в рантайме.
    }
}
