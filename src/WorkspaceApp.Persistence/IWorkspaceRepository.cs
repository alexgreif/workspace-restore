using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

/// <summary>
/// Persistence boundary for workspace CRUD and listing operations.
/// </summary>
public interface IWorkspaceRepository
{
    IReadOnlyList<WorkspaceSummary> List();

    /// <summary>
    /// Loads a single workspace by id and fails when the record is missing or invalid.
    /// </summary>
    Workspace Get(WorkspaceId id);

    WorkspaceId Create(Workspace workspace);

    void Update(Workspace workspace);

    void Delete(WorkspaceId id);
}
