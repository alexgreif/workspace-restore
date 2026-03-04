using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

public interface ISchemaMigrator
{
    Workspace MigrateToLatest(Workspace workspace);

    SchemaVersion LatestVersion();
}
