using System.Runtime.InteropServices;

namespace Server.Infrastructure.Execution;

internal static class WindowsNativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool LockWorkStation();
}
