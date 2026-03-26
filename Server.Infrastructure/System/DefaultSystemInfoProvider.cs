using System.Runtime.InteropServices;
using Server.Application;
using Server.Application.Contracts;

namespace Server.Infrastructure.System;

/// <summary>
/// Провайдер базовой информации о системе.
/// </summary>
public sealed class DefaultSystemInfoProvider : ISystemInfoProvider
{
    /// <inheritdoc />
    public Task<SystemInfoDto> GetAsync()
    {
        var dto = new SystemInfoDto(
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            DateTimeOffset.UtcNow);
        return Task.FromResult(dto);
    }
}
