using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence.Storage;

public sealed class LocalAppDataWorkspaceStoragePathProvider : IWorkspaceStoragePathProvider
{
    public LocalAppDataWorkspaceStoragePathProvider(string? localAppDataPath = null)
    {
        var resolvedLocalAppDataPath = localAppDataPath;
        if (string.IsNullOrWhiteSpace(resolvedLocalAppDataPath))
        {
            resolvedLocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (string.IsNullOrWhiteSpace(resolvedLocalAppDataPath))
        {
            throw new InvalidOperationException("Unable to resolve LocalAppData path.");
        }

        WorkspaceDirectoryPath = Path.Combine(
            resolvedLocalAppDataPath,
            WorkspaceStorageConstants.AppDirectoryName,
            WorkspaceStorageConstants.WorkspacesDirectoryName);
    }

    public string WorkspaceDirectoryPath { get; }

    public string GetWorkspaceFilePath(WorkspaceId workspaceId)
    {
        if (string.IsNullOrWhiteSpace(workspaceId.Value))
        {
            throw new ArgumentException("WorkspaceId cannot be empty.", nameof(workspaceId));
        }

        var fileName = workspaceId.Value + WorkspaceStorageConstants.WorkspaceFileExtension;
        return Path.Combine(WorkspaceDirectoryPath, fileName);
    }
}
