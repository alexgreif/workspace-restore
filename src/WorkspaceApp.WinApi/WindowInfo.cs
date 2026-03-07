namespace WorkspaceApp.WinApi;

/// <summary>
/// Snapshot metadata for one top-level window discovered during enumeration.
/// </summary>
public sealed class WindowInfo
{
    public WindowInfo(
        IntPtr hwnd,
        int processId,
        WinApiRect bounds,
        string? title,
        string? className,
        bool isVisible,
        bool isCloaked,
        bool isShellInfrastructure,
        string? exePath,
        long zOrderRank)
    {
        if (processId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(processId), "ProcessId must be zero or greater.");
        }

        if (zOrderRank < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(zOrderRank), "ZOrderRank must be zero or greater.");
        }

        Hwnd = hwnd;
        ProcessId = processId;
        Bounds = bounds;
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        ClassName = string.IsNullOrWhiteSpace(className) ? null : className.Trim();
        IsVisible = isVisible;
        IsCloaked = isCloaked;
        IsShellInfrastructure = isShellInfrastructure;
        ExePath = string.IsNullOrWhiteSpace(exePath) ? null : exePath.Trim();
        ZOrderRank = zOrderRank;
    }

    public IntPtr Hwnd { get; }

    public int ProcessId { get; }

    public WinApiRect Bounds { get; }

    public string? Title { get; }

    public string? ClassName { get; }

    public bool IsVisible { get; }

    public bool IsCloaked { get; }

    public bool IsShellInfrastructure { get; }

    public string? ExePath { get; }

    public long ZOrderRank { get; }
}
