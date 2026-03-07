namespace WorkspaceApp.WinApi;

/// <summary>
/// WinApi-local rectangle in virtual desktop coordinates.
/// </summary>
public readonly record struct WinApiRect
{
    public WinApiRect(int x, int y, int width, int height)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be zero or greater.");
        }

        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be zero or greater.");
        }

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int X { get; }

    public int Y { get; }

    public int Width { get; }

    public int Height { get; }
}
