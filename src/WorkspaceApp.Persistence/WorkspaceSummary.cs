using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

public sealed class WorkspaceSummary
{
    public WorkspaceSummary(WorkspaceId id, string name, DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("WorkspaceId cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        if (updatedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("UpdatedAtUtc must be in UTC.", nameof(updatedAtUtc));
        }

        Id = id;
        Name = name.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public WorkspaceId Id { get; }

    public string Name { get; }

    public DateTimeOffset UpdatedAtUtc { get; }
}
