using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

public sealed class SchemaMigratorV1 : ISchemaMigrator
{
    public Workspace MigrateToLatest(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return workspace;
    }

    public SchemaVersion LatestVersion() => SchemaVersion.V1;
}
