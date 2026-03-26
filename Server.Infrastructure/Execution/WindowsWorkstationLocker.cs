using System.ComponentModel;
using System.Runtime.InteropServices;
using Server.Application.Contracts;

namespace Server.Infrastructure.Execution;

/// <summary>
/// Реализация блокировки рабочей станции для Windows.
/// </summary>
public sealed class WindowsWorkstationLocker : IWorkstationLocker
{
    /// <inheritdoc />
    public void Lock()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Workstation lock is supported only on Windows.");
        }

        if (!WindowsNativeMethods.LockWorkStation())
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to lock workstation.");
        }
    }
}
