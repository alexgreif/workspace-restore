using WorkspaceApp.Domain.Models;
using WorkspaceApp.Persistence.Storage;

namespace WorkspaceApp.Persistence.Tests;

public sealed class JsonWorkspaceRepositoryTests
{
    [Fact]
    public void Create_ThenGet_RoundTripsWorkspace()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);
        var workspace = CreateSampleWorkspace(
            new WorkspaceId("11111111-1111-1111-1111-111111111111"),
            "Dev Setup",
            DateTimeOffset.Parse("2026-03-04T10:00:00+00:00"));

        repository.Create(workspace);
        var loaded = repository.Get(workspace.Id);

        AssertWorkspacesEqual(workspace, loaded);
    }

    [Fact]
    public void Create_WritesWorkspaceJsonToExpectedLocalAppDataLayout()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);
        var workspace = CreateSampleWorkspace(
            new WorkspaceId("22222222-2222-2222-2222-222222222222"),
            "Workspace A",
            DateTimeOffset.Parse("2026-03-04T11:00:00+00:00"));

        repository.Create(workspace);

        var expectedDirectory = Path.Combine(
            tempDir.Path,
            WorkspaceStorageConstants.AppDirectoryName,
            WorkspaceStorageConstants.WorkspacesDirectoryName);
        var expectedFile = Path.Combine(
            expectedDirectory,
            workspace.Id.Value + WorkspaceStorageConstants.WorkspaceFileExtension);

        Assert.True(Directory.Exists(expectedDirectory));
        Assert.True(File.Exists(expectedFile));
    }

    [Fact]
    public void List_ReturnsCorrectSummaries()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);

        var workspaceB = CreateSampleWorkspace(
            new WorkspaceId("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Beta",
            DateTimeOffset.Parse("2026-03-04T12:00:00+00:00"));
        var workspaceA = CreateSampleWorkspace(
            new WorkspaceId("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Alpha",
            DateTimeOffset.Parse("2026-03-04T13:00:00+00:00"));

        repository.Create(workspaceB);
        repository.Create(workspaceA);

        var summaries = repository.List();

        Assert.Equal(2, summaries.Count);
        Assert.Equal(workspaceA.Id, summaries[0].Id);
        Assert.Equal(workspaceA.Name, summaries[0].Name);
        Assert.Equal(workspaceA.UpdatedAtUtc, summaries[0].UpdatedAtUtc);
        Assert.Equal(workspaceB.Id, summaries[1].Id);
    }

    [Fact]
    public void Update_OverwritesWorkspaceFile()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);
        var id = new WorkspaceId("33333333-3333-3333-3333-333333333333");

        var initial = CreateSampleWorkspace(
            id,
            "Before",
            DateTimeOffset.Parse("2026-03-04T14:00:00+00:00"));
        repository.Create(initial);

        var updated = CreateSampleWorkspace(
            id,
            "After",
            DateTimeOffset.Parse("2026-03-04T15:00:00+00:00"));
        repository.Update(updated);

        var loaded = repository.Get(id);
        Assert.Equal("After", loaded.Name);
        Assert.Equal(updated.UpdatedAtUtc, loaded.UpdatedAtUtc);
    }

    [Fact]
    public void Delete_RemovesWorkspaceFile()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);
        var workspace = CreateSampleWorkspace(
            new WorkspaceId("44444444-4444-4444-4444-444444444444"),
            "To Delete",
            DateTimeOffset.Parse("2026-03-04T16:00:00+00:00"));

        repository.Create(workspace);
        repository.Delete(workspace.Id);

        Assert.Empty(repository.List());
        Assert.Throws<FileNotFoundException>(() => repository.Get(workspace.Id));
    }

    [Fact]
    public void Get_WhenWorkspaceDoesNotExist_ThrowsFileNotFoundException()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);
        var missingId = new WorkspaceId("55555555-5555-5555-5555-555555555555");

        Assert.Throws<FileNotFoundException>(() => repository.Get(missingId));
    }

    [Fact]
    public void Create_WhenWorkspaceAlreadyExists_ThrowsInvalidOperationException()
    {
        using var tempDir = new TempDirectory();
        var repository = CreateRepository(tempDir.Path);
        var workspace = CreateSampleWorkspace(
            new WorkspaceId("66666666-6666-6666-6666-666666666666"),
            "Duplicate",
            DateTimeOffset.Parse("2026-03-04T17:00:00+00:00"));

        repository.Create(workspace);

        Assert.Throws<InvalidOperationException>(() => repository.Create(workspace));
    }

    [Fact]
    public void Get_WhenWorkspaceFileContainsInvalidJson_ThrowsInvalidDataException()
    {
        using var tempDir = new TempDirectory();
        var storage = new LocalAppDataWorkspaceStoragePathProvider(tempDir.Path);
        var repository = new JsonWorkspaceRepository(storagePathProvider: storage);
        var id = new WorkspaceId("77777777-7777-7777-7777-777777777777");

        Directory.CreateDirectory(storage.WorkspaceDirectoryPath);
        File.WriteAllText(storage.GetWorkspaceFilePath(id), "{ invalid json");

        Assert.Throws<InvalidDataException>(() => repository.Get(id));
    }

    [Fact]
    public void List_SkipsCorruptedWorkspaceFiles_AndLogsWarning()
    {
        using var tempDir = new TempDirectory();
        var warnings = new List<string>();
        var storage = new LocalAppDataWorkspaceStoragePathProvider(tempDir.Path);
        var repository = new JsonWorkspaceRepository(
            storagePathProvider: storage,
            logWarning: warnings.Add);

        var validWorkspace = CreateSampleWorkspace(
            new WorkspaceId("99999999-9999-9999-9999-999999999999"),
            "Valid",
            DateTimeOffset.Parse("2026-03-04T18:30:00+00:00"));
        repository.Create(validWorkspace);

        var corruptedId = new WorkspaceId("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        File.WriteAllText(storage.GetWorkspaceFilePath(corruptedId), "{ invalid json");

        var summaries = repository.List();

        Assert.Single(summaries);
        Assert.Equal(validWorkspace.Id, summaries[0].Id);
        Assert.Single(warnings);
        Assert.Contains("Skipping corrupted workspace file", warnings[0]);
    }

    [Fact]
    public void Get_UsesSchemaMigrator()
    {
        using var tempDir = new TempDirectory();
        var storage = new LocalAppDataWorkspaceStoragePathProvider(tempDir.Path);
        // Keep seed-write behavior independent so this test only validates read-time migration.
        var sourceRepository = new JsonWorkspaceRepository(storagePathProvider: storage);
        var workspace = CreateSampleWorkspace(
            new WorkspaceId("88888888-8888-8888-8888-888888888888"),
            "Original Name",
            DateTimeOffset.Parse("2026-03-04T18:00:00+00:00"));
        sourceRepository.Create(workspace);

        // Return a clearly different object to prove Get() uses migrator output, not raw deserialized data.
        var migrated = CreateSampleWorkspace(
            workspace.Id,
            "Migrated Name",
            DateTimeOffset.Parse("2026-03-04T19:00:00+00:00"));
        var migrator = new TestSchemaMigrator(migrated);
        // Reuse the same storage so this assertion isolates migration behavior rather than storage differences.
        var repository = new JsonWorkspaceRepository(migrator, storage);

        var loaded = repository.Get(workspace.Id);

        Assert.Equal("Migrated Name", loaded.Name);
        Assert.Equal(1, migrator.MigrateToLatestCalls);
    }

    private static JsonWorkspaceRepository CreateRepository(string localAppDataPath)
    {
        var storagePathProvider = new LocalAppDataWorkspaceStoragePathProvider(localAppDataPath);
        return new JsonWorkspaceRepository(storagePathProvider: storagePathProvider);
    }

    private static Workspace CreateSampleWorkspace(WorkspaceId id, string name, DateTimeOffset updatedAtUtc)
    {
        var createdAtUtc = updatedAtUtc.AddMinutes(-5);

        return new Workspace(
            id,
            name,
            createdAtUtc,
            updatedAtUtc,
            [
                new ApplicationEntry(
                    @"C:\Apps\Editor.exe",
                    [
                        new WindowLayout(new Rect(100, 200, 800, 600), 0, new MonitorHint(0), "Editor - Project"),
                        new WindowLayout(new Rect(120, 240, 600, 400), 1, null, "Terminal")
                    ])
            ],
            SchemaVersion.V1);
    }

    private static void AssertWorkspacesEqual(Workspace expected, Workspace actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.CreatedAtUtc, actual.CreatedAtUtc);
        Assert.Equal(expected.UpdatedAtUtc, actual.UpdatedAtUtc);
        Assert.Equal(expected.SchemaVersion, actual.SchemaVersion);
        Assert.Equal(expected.Entries.Count, actual.Entries.Count);

        for (var entryIndex = 0; entryIndex < expected.Entries.Count; entryIndex++)
        {
            var expectedEntry = expected.Entries[entryIndex];
            var actualEntry = actual.Entries[entryIndex];

            Assert.Equal(expectedEntry.ExePath, actualEntry.ExePath);
            Assert.Equal(expectedEntry.Windows.Count, actualEntry.Windows.Count);

            for (var windowIndex = 0; windowIndex < expectedEntry.Windows.Count; windowIndex++)
            {
                var expectedWindow = expectedEntry.Windows[windowIndex];
                var actualWindow = actualEntry.Windows[windowIndex];

                Assert.Equal(expectedWindow.Bounds, actualWindow.Bounds);
                Assert.Equal(expectedWindow.ZOrderIndex, actualWindow.ZOrderIndex);
                Assert.Equal(expectedWindow.TitleHint, actualWindow.TitleHint);
                Assert.Equal(expectedWindow.MonitorHint?.MonitorIndex, actualWindow.MonitorHint?.MonitorIndex);
            }
        }
    }

    // Test double: makes migration behavior deterministic so we can assert Get() calls migrator and returns its result.
    private sealed class TestSchemaMigrator : ISchemaMigrator
    {
        private readonly Workspace _migratedWorkspace;

        public TestSchemaMigrator(Workspace migratedWorkspace)
        {
            _migratedWorkspace = migratedWorkspace;
        }

        public int MigrateToLatestCalls { get; private set; }

        public Workspace MigrateToLatest(Workspace workspace)
        {
            MigrateToLatestCalls++;
            return _migratedWorkspace;
        }

        public SchemaVersion LatestVersion() => SchemaVersion.V1;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            // Unique per-test root prevents cross-test contamination when tests run in parallel.
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "workspaceapp-persistence-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
