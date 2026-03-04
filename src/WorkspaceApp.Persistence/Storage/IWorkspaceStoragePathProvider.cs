using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence.Storage;

/// <summary>
/// Resolves directory and file locations for persisted workspace documents.
/// </summary>
public interface IWorkspaceStoragePathProvider
{
    string WorkspaceDirectoryPath { get; }

    string GetWorkspaceFilePath(WorkspaceId workspaceId);
}
