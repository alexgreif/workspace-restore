namespace WorkspaceApp.Domain.Models;

/// <summary>
/// Represents a persisted workspace configuration, including metadata and application entries.
/// </summary>
public sealed class Workspace
{
    public Workspace(
        WorkspaceId id,
        string name,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        IEnumerable<ApplicationEntry> entries,
        SchemaVersion schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("WorkspaceId cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        // "*Utc" fields are contractually required to be UTC instants.
        if (createdAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("CreatedAtUtc must be in UTC.", nameof(createdAtUtc));
        }

        if (updatedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("UpdatedAtUtc must be in UTC.", nameof(updatedAtUtc));
        }

        if (updatedAtUtc < createdAtUtc)
        {
            throw new ArgumentException("UpdatedAtUtc cannot be earlier than CreatedAtUtc.", nameof(updatedAtUtc));
        }

        ArgumentNullException.ThrowIfNull(entries);

        Id = id;
        Name = name.Trim();
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        Entries = entries.ToList().AsReadOnly();
        SchemaVersion = schemaVersion;
    }

    public WorkspaceId Id { get; }

    public string Name { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; }

    public IReadOnlyList<ApplicationEntry> Entries { get; }

    public SchemaVersion SchemaVersion { get; }
}
