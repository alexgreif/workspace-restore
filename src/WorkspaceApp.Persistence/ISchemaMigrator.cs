using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

/// <summary>
/// Upgrades deserialized workspace documents to the latest supported schema.
/// </summary>
public interface ISchemaMigrator
{
    /// <summary>
    /// Migrates a workspace instance to <see cref="LatestVersion"/>.
    /// </summary>
    Workspace MigrateToLatest(Workspace workspace);

    /// <summary>
    /// Returns the highest schema version this binary understands.
    /// </summary>
    SchemaVersion LatestVersion();
}
