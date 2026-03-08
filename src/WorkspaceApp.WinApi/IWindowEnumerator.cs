namespace WorkspaceApp.WinApi;

/// <summary>
/// Enumerates top-level windows and returns snapshot metadata for each included window.
/// </summary>
public interface IWindowEnumerator
{
    List<WindowInfo> EnumerateTopLevelWindows(WindowFilter filter);
}
