using Server.Application.Contracts;

namespace Server.Infrastructure.Execution;

/// <summary>
/// Заглушка, если <c>Notifications:ToastListenerBaseUrl</c> не задан.
/// </summary>
public sealed class NoOpToastListenerForwarder : IToastListenerForwarder
{
    /// <inheritdoc />
    public Task ForwardAsync(string title, string? message, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
