namespace WorkspaceApp.Domain.Models;

public sealed class WindowLayout
{
    public WindowLayout(Rect bounds, int zOrderIndex, MonitorHint? monitorHint = null, string? titleHint = null)
    {
        if (zOrderIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(zOrderIndex),
                "ZOrderIndex must be zero or greater."
            );
        }

        Bounds = bounds;
        ZOrderIndex = zOrderIndex;
        MonitorHint = monitorHint;
        TitleHint = string.IsNullOrWhiteSpace(titleHint) ? null : titleHint.Trim();
    }

    public Rect Bounds { get; }

    public int ZOrderIndex { get; }

    public MonitorHint? MonitorHint { get; }

    public string? TitleHint { get; }
}
