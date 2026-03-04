using System.Text;
using System.Text.Json;
using WorkspaceApp.Domain.Models;
using WorkspaceApp.Persistence.Serialization;
using WorkspaceApp.Persistence.Storage;

namespace WorkspaceApp.Persistence;

public sealed class JsonWorkspaceRepository : IWorkspaceRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly Action<string> _logWarning;
    private readonly ISchemaMigrator _schemaMigrator;
    private readonly IWorkspaceStoragePathProvider _storagePathProvider;

    public JsonWorkspaceRepository(
        ISchemaMigrator? schemaMigrator = null,
        IWorkspaceStoragePathProvider? storagePathProvider = null,
        Action<string>? logWarning = null)
    {
        _logWarning = logWarning ?? (_ => { });
        _schemaMigrator = schemaMigrator ?? new SchemaMigratorV1();
        _storagePathProvider = storagePathProvider ?? new LocalAppDataWorkspaceStoragePathProvider();
    }

    public IReadOnlyList<WorkspaceSummary> List()
    {
        if (!Directory.Exists(_storagePathProvider.WorkspaceDirectoryPath))
        {
            return [];
        }

        var summaries = new List<WorkspaceSummary>();
        var workspaceFiles = Directory.EnumerateFiles(
            _storagePathProvider.WorkspaceDirectoryPath,
            "*" + WorkspaceStorageConstants.WorkspaceFileExtension,
            SearchOption.TopDirectoryOnly);

        foreach (var filePath in workspaceFiles)
        {
            try
            {
                var workspace = ReadWorkspaceFromFile(filePath);
                summaries.Add(new WorkspaceSummary(workspace.Id, workspace.Name, workspace.UpdatedAtUtc));
            }
            catch (InvalidDataException ex)
            {
                // Listing should remain available even if one file is corrupted.
                _logWarning($"Skipping corrupted workspace file '{filePath}': {ex.Message}");
            }
        }

        summaries = summaries
            .OrderBy(summary => summary.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.Id.Value, StringComparer.Ordinal)
            .ToList();

        return summaries.AsReadOnly();
    }

    public Workspace Get(WorkspaceId id)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("WorkspaceId cannot be empty.", nameof(id));
        }

        var filePath = _storagePathProvider.GetWorkspaceFilePath(id);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Workspace '{id.Value}' was not found.", filePath);
        }

        return ReadWorkspaceFromFile(filePath);
    }

    public WorkspaceId Create(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        EnsureWorkspaceDirectoryExists();

        var filePath = _storagePathProvider.GetWorkspaceFilePath(workspace.Id);
        if (File.Exists(filePath))
        {
            throw new InvalidOperationException($"Workspace '{workspace.Id.Value}' already exists.");
        }

        WriteWorkspaceToFile(filePath, workspace, overwrite: false);
        return workspace.Id;
    }

    public void Update(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        EnsureWorkspaceDirectoryExists();

        var filePath = _storagePathProvider.GetWorkspaceFilePath(workspace.Id);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Workspace '{workspace.Id.Value}' was not found.", filePath);
        }

        WriteWorkspaceToFile(filePath, workspace, overwrite: true);
    }

    public void Delete(WorkspaceId id)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("WorkspaceId cannot be empty.", nameof(id));
        }

        var filePath = _storagePathProvider.GetWorkspaceFilePath(id);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Workspace '{id.Value}' was not found.", filePath);
        }

        File.Delete(filePath);
    }

    private Workspace ReadWorkspaceFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            var document = JsonSerializer.Deserialize<WorkspaceDocument>(json, WorkspaceJsonSerializerOptions.Default);
            if (document is null)
            {
                throw new InvalidDataException($"Workspace file '{filePath}' is empty or invalid.");
            }

            var workspace = WorkspaceJsonMapper.ToDomain(document);
            return _schemaMigrator.MigrateToLatest(workspace);
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or ArgumentException)
        {
            // Normalize parse/validation failures so callers can handle one persistence error category.
            throw new InvalidDataException($"Failed to read workspace file '{filePath}'.", ex);
        }
    }

    private void WriteWorkspaceToFile(string filePath, Workspace workspace, bool overwrite)
    {
        var document = WorkspaceJsonMapper.ToDocument(workspace);
        var json = JsonSerializer.Serialize(document, WorkspaceJsonSerializerOptions.Default);

        if (overwrite)
        {
            File.WriteAllText(filePath, json, Utf8NoBom);
            return;
        }

        // CreateNew enforces create semantics atomically and prevents accidental overwrite.
        using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, Utf8NoBom);
        writer.Write(json);
    }

    private void EnsureWorkspaceDirectoryExists()
    {
        Directory.CreateDirectory(_storagePathProvider.WorkspaceDirectoryPath);
    }
}
