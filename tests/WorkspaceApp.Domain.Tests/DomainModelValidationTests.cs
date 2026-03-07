using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Domain.Tests;

public sealed class DomainModelValidationTests
{
    private static readonly WindowLayout SampleWindow = new(new Rect(0, 0, 100, 100), 0);

    [Fact]
    public void Workspace_WithValidData_CreatesSuccessfully()
    {
        var window = new WindowLayout(new Rect(100, 100, 800, 600), 0);
        var entry = new ApplicationEntry(@"C:\Apps\Tool.exe", [window]);
        var nowUtc = DateTimeOffset.UtcNow;

        var workspace = new Workspace(
            WorkspaceId.New(),
            "Dev Setup",
            nowUtc,
            nowUtc,
            [entry],
            SchemaVersion.V1);

        Assert.Equal("Dev Setup", workspace.Name);
        Assert.Single(workspace.Entries);
        Assert.Equal(SchemaVersion.V1, workspace.SchemaVersion);
    }

    [Fact]
    public void Workspace_WithNonUtcCreatedAt_ThrowsArgumentException()
    {
        var window = new WindowLayout(new Rect(100, 100, 800, 600), 0);
        var entry = new ApplicationEntry(@"C:\Apps\Tool.exe", [window]);
        // Generate a non-UTC offset deterministically without relying on OS-specific time zone IDs.
        var nowNonUtc = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(1));
        var nowUtc = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(
            () => new Workspace(
                WorkspaceId.New(),
                "Dev Setup",
                nowNonUtc,
                nowUtc,
                [entry],
                SchemaVersion.V1
            )
        );
    }

    [Fact]
    public void Rect_WithNonPositiveWidth_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rect(0, 0, 0, 100));
    }

    [Fact]
    public void ApplicationEntry_WithRelativeExePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ApplicationEntry("Tool.exe", [SampleWindow]));
    }

    [Fact]
    public void ApplicationEntry_WithNullExePath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ApplicationEntry(null!, [SampleWindow]));
    }

    [Fact]
    public void ApplicationEntry_WithAbsoluteNonExePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ApplicationEntry(@"C:\Apps\Tool.txt", [SampleWindow]));
    }

    [Fact]
    public void ApplicationEntry_WithTrimmedUppercaseExePath_CreatesSuccessfully()
    {
        var entry = new ApplicationEntry(@"  C:\Apps\Tool.EXE  ", [SampleWindow]);

        Assert.Equal(@"C:\Apps\Tool.EXE", entry.ExePath);
    }

    [Fact]
    public void ApplicationEntry_WithWhitespaceOnlyExePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ApplicationEntry("   ", [SampleWindow]));
    }

    [Fact]
    public void ApplicationEntry_WithEmptyWindows_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new ApplicationEntry(@"C:\Apps\Tool.exe", []));
    }
}
