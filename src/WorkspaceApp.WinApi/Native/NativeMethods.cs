using System.Runtime.InteropServices;
using System.Text;

namespace WorkspaceApp.WinApi.Native;

internal static class NativeMethods
{
    // Conservative fixed-size buffers for window text/class retrieval.
    // Enum-time metadata is best-effort, so truncation is acceptable in V1.
    internal const int MaxWindowTextLength = 1024;
    internal const int MaxClassNameLength = 256;
    internal const int DwmWindowAttributeCloaked = 14; // DWMWA_CLOAKED

    // EnumWindows traverses top-level windows in z-order.
    // The enumerator implementation can reuse this ordering to derive ZOrderRank.
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    // SetLastError is enabled on Win32 calls so failures can be diagnosed via Marshal.GetLastWin32Error().
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

    // Unicode entrypoints avoid locale-dependent behavior and align with modern Windows text APIs.
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int GetClassNameW(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // DWM cloaking helps exclude windows that are technically present but not user-visible.
    [DllImport("dwmapi.dll", PreserveSig = true)]
    internal static extern int DwmGetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        out int pvAttribute,
        int cbAttribute);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeRect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
