using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence.Storage;

public interface IWorkspaceStoragePathProvider
{
    string WorkspaceDirectoryPath { get; }

    string GetWorkspaceFilePath(WorkspaceId workspaceId);
}
