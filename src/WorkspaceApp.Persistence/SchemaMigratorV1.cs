using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

public sealed class SchemaMigratorV1 : ISchemaMigrator
{
    public Workspace MigrateToLatest(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        // V1 has no historical versions to upgrade from yet.
        return workspace;
    }

    public SchemaVersion LatestVersion() => SchemaVersion.V1;
}
