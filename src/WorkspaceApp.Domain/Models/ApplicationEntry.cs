namespace WorkspaceApp.Domain.Models;

/// <summary>
/// Represents one executable and its captured window layouts within a workspace.
/// </summary>
public sealed class ApplicationEntry
{
    public ApplicationEntry(string exePath, IEnumerable<WindowLayout> windows)
    {
        if (exePath is null)
        {
            throw new ArgumentNullException(
                nameof(exePath),
                "exePath cannot be null."
            );
        }
        
        var exePathTrimmed = exePath.Trim();

        // Domain validation is intentionally format-based only.
        // The file may no longer exist when a persisted workspace is restored later.
        if (!Path.IsPathFullyQualified(exePathTrimmed))
        {
            throw new ArgumentException(
                "exePath must be a fully qualified Windows path.",
                nameof(exePath)
            );
        }
        
        if (!exePathTrimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "exePath must point to a .exe file.",
                nameof(exePath)
            );
        }

        ArgumentNullException.ThrowIfNull(windows);

        var windowList = windows.ToList();
        if (windowList.Count == 0)
        {
            throw new ArgumentException(
                "ApplicationEntry must contain at least one window.",
                nameof(windows)
            );
        }

        ExePath = exePathTrimmed;
        Windows = windowList.AsReadOnly();
    }

    public string ExePath { get; }

    public IReadOnlyList<WindowLayout> Windows { get; }
}
