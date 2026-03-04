using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence.Serialization;

public static class WorkspaceJsonMapper
{
    public static WorkspaceDocument ToDocument(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return new WorkspaceDocument
        {
            Id = workspace.Id,
            Name = workspace.Name,
            CreatedAtUtc = workspace.CreatedAtUtc,
            UpdatedAtUtc = workspace.UpdatedAtUtc,
            SchemaVersion = workspace.SchemaVersion,
            Entries = workspace.Entries
                .Select(entry => new ApplicationEntryDocument
                {
                    ExePath = entry.ExePath,
                    Windows = entry.Windows
                        .Select(window => new WindowLayoutDocument
                        {
                            Bounds = new RectDocument
                            {
                                X = window.Bounds.X,
                                Y = window.Bounds.Y,
                                Width = window.Bounds.Width,
                                Height = window.Bounds.Height
                            },
                            ZOrderIndex = window.ZOrderIndex,
                            MonitorHint = window.MonitorHint is null
                                ? null
                                : new MonitorHintDocument
                                {
                                    MonitorIndex = window.MonitorHint.MonitorIndex
                                },
                            TitleHint = window.TitleHint
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public static Workspace ToDomain(WorkspaceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new Workspace(
            document.Id,
            document.Name,
            document.CreatedAtUtc,
            document.UpdatedAtUtc,
            document.Entries.Select(entry => new ApplicationEntry(
                entry.ExePath,
                entry.Windows.Select(window => new WindowLayout(
                    new Rect(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height),
                    window.ZOrderIndex,
                    window.MonitorHint is null ? null : new MonitorHint(window.MonitorHint.MonitorIndex),
                    window.TitleHint)))),
            document.SchemaVersion);
    }
}
