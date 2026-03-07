namespace WorkspaceApp.WinApi;

/// <summary>
/// Filter options for top-level window enumeration.
/// </summary>
public sealed class WindowFilter
{
    public bool RequireVisible { get; init; } = true;

    public bool ExcludeCloaked { get; init; } = true;

    public bool ExcludeShellInfrastructure { get; init; } = true;
}
