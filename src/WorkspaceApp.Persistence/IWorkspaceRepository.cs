using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence;

public interface IWorkspaceRepository
{
    IReadOnlyList<WorkspaceSummary> List();

    Workspace Get(WorkspaceId id);

    WorkspaceId Create(Workspace workspace);

    void Update(Workspace workspace);

    void Delete(WorkspaceId id);
}
