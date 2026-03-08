using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WorkspaceApp.WinApi.Native;

namespace WorkspaceApp.WinApi;

public sealed class WindowEnumerator : IWindowEnumerator
{
    private static readonly HashSet<string> ShellInfrastructureClassNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Shell_TrayWnd",
        "Progman",
        "WorkerW",
        "Shell_SecondaryTrayWnd"
    };

    public List<WindowInfo> EnumerateTopLevelWindows(WindowFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var windows = new List<WindowInfo>();
        var exePathByProcessId = new Dictionary<int, string?>();
        var zOrderRank = 0L;

        var enumSucceeded = NativeMethods.EnumWindows(
            (hWnd, _) =>
            {
                if (TryCreateWindowInfo(hWnd, filter, zOrderRank, exePathByProcessId, out var windowInfo))
                {
                    windows.Add(windowInfo);
                    zOrderRank++;
                }

                // Continue enumeration; per-window failures are handled as best-effort skips.
                return true;
            },
            IntPtr.Zero);

        if (!enumSucceeded)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                "EnumWindows failed while enumerating top-level windows.");
        }

        return windows;
    }

    private static bool TryCreateWindowInfo(
        IntPtr hWnd,
        WindowFilter filter,
        long zOrderRank,
        Dictionary<int, string?> exePathByProcessId,
        out WindowInfo windowInfo)
    {
        windowInfo = null!;

        var isVisible = NativeMethods.IsWindowVisible(hWnd);
        if (filter.RequireVisible && !isVisible)
        {
            return false;
        }

        var className = TryGetClassName(hWnd);
        var isShellInfrastructure = IsShellInfrastructure(className);
        if (filter.ExcludeShellInfrastructure && isShellInfrastructure)
        {
            return false;
        }

        var isCloaked = TryIsWindowCloaked(hWnd, out var cloaked) && cloaked;
        if (filter.ExcludeCloaked && isCloaked)
        {
            return false;
        }

        if (!NativeMethods.GetWindowRect(hWnd, out var nativeRect))
        {
            return false;
        }

        var width = nativeRect.Right - nativeRect.Left;
        var height = nativeRect.Bottom - nativeRect.Top;
        if (width < 0 || height < 0)
        {
            return false;
        }

        NativeMethods.GetWindowThreadProcessId(hWnd, out var rawProcessId);
        // Null means unavailable/unrepresentable PID:
        // - raw PID is 0 (API failure/no associated process)
        // - raw PID exceeds Int32 range used by Process.GetProcessById.
        int? processId = rawProcessId switch
        {
            0 => null,
            <= int.MaxValue => (int)rawProcessId,
            _ => null
        };

        windowInfo = new WindowInfo(
            hWnd,
            processId,
            new WinApiRect(nativeRect.Left, nativeRect.Top, width, height),
            GetWindowShowState(hWnd),
            TryGetWindowTitle(hWnd),
            className,
            isVisible,
            isCloaked,
            isShellInfrastructure,
            GetExePathFromCache(processId, exePathByProcessId),
            zOrderRank);

        return true;
    }

    private static string? TryGetWindowTitle(IntPtr hWnd)
    {
        var buffer = new StringBuilder(NativeMethods.MaxWindowTextLength);
        var length = NativeMethods.GetWindowTextW(hWnd, buffer, buffer.Capacity);
        if (length <= 0)
        {
            return null;
        }

        return buffer.ToString(0, length);
    }

    private static string? TryGetClassName(IntPtr hWnd)
    {
        var buffer = new StringBuilder(NativeMethods.MaxClassNameLength);
        var length = NativeMethods.GetClassNameW(hWnd, buffer, buffer.Capacity);
        if (length <= 0)
        {
            return null;
        }

        return buffer.ToString(0, length);
    }

    private static string? TryGetExePath(int? processId)
    {
        if (!processId.HasValue)
        {
            return null;
        }

        try
        {
            using var process = Process.GetProcessById(processId.Value);
            return process.MainModule?.FileName;
        }
        catch (ArgumentException)
        {
            // PID disappeared between enumeration and lookup.
            return null;
        }
        catch (InvalidOperationException)
        {
            // Process exited while querying process metadata.
            return null;
        }
        catch (Win32Exception)
        {
            // Access denied or other OS-level process metadata read failure.
            return null;
        }
    }

    private static string? GetExePathFromCache(int? processId, Dictionary<int, string?> exePathByProcessId)
    {
        if (!processId.HasValue)
        {
            return null;
        }

        if (exePathByProcessId.TryGetValue(processId.Value, out var cachedExePath))
        {
            return cachedExePath;
        }

        var resolvedExePath = TryGetExePath(processId.Value);
        exePathByProcessId[processId.Value] = resolvedExePath;
        return resolvedExePath;
    }

    private static bool TryIsWindowCloaked(IntPtr hWnd, out bool isCloaked)
    {
        var hr = NativeMethods.DwmGetWindowAttribute(
            hWnd,
            NativeMethods.DwmWindowAttributeCloaked,
            out var cloakedValue,
            sizeof(int));

        // fallback to best-effort if DWM query fails
        if (hr != 0)
        {
            isCloaked = false;
            return false;
        }

        isCloaked = cloakedValue != 0;
        return true;
    }

    private static WindowShowState GetWindowShowState(IntPtr hWnd)
    {
        if (NativeMethods.IsIconic(hWnd))
        {
            return WindowShowState.Minimized;
        }

        if (NativeMethods.IsZoomed(hWnd))
        {
            return WindowShowState.Maximized;
        }

        return WindowShowState.Normal;
    }

    private static bool IsShellInfrastructure(string? className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return false;
        }

        return ShellInfrastructureClassNames.Contains(className);
    }
}
