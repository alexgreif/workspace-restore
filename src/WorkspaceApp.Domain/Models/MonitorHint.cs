namespace WorkspaceApp.Domain.Models;

public sealed record MonitorHint
{
    public MonitorHint(int? monitorIndex)
    {
        if (monitorIndex is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monitorIndex),
                "MonitorIndex must be zero or greater when provided."
            );
        }

        MonitorIndex = monitorIndex;
    }

    public int? MonitorIndex { get; }
}
