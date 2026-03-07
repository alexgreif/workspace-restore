using WorkspaceApp.WinApi;

var argsSet = new HashSet<string>(args, StringComparer.OrdinalIgnoreCase);
if (argsSet.Contains("--help") || argsSet.Contains("-h"))
{
    PrintUsage();
    return;
}

if (!TryParseLimit(args, out var limit, out var limitParseError))
{
    Console.Error.WriteLine(limitParseError);
    Console.Error.WriteLine();
    PrintUsage();
    return;
}

var filter = new WindowFilter
{
    RequireVisible = !argsSet.Contains("--include-hidden"),
    ExcludeCloaked = !argsSet.Contains("--include-cloaked"),
    ExcludeShellInfrastructure = !argsSet.Contains("--include-shell")
};

var enumerator = new WindowEnumerator();
var windows = enumerator.EnumerateTopLevelWindows(filter);

Console.WriteLine(
    $"Snapshot: {windows.Count} window(s) | " +
    $"RequireVisible={filter.RequireVisible}, " +
    $"ExcludeCloaked={filter.ExcludeCloaked}, " +
    $"ExcludeShellInfrastructure={filter.ExcludeShellInfrastructure}" +
    (limit.HasValue ? $" | Limit={limit.Value}" : string.Empty));
Console.WriteLine();

var windowsToPrint = limit.HasValue ? Math.Min(limit.Value, windows.Count) : windows.Count;
for (var i = 0; i < windowsToPrint; i++)
{
    var window = windows[i];

    Console.WriteLine(
        $"[{i:00}] z={window.ZOrderRank} hwnd=0x{window.Hwnd.ToInt64():X} " +
        $"pid={FormatPid(window.ProcessId)} proc={FormatProcessName(window.ExePath)}");

    Console.WriteLine($"title : \"{Truncate(window.Title, 120)}\"");
    Console.WriteLine($"class : \"{Truncate(window.ClassName, 64)}\"");

    Console.WriteLine(
        $"state : visible={TF(window.IsVisible),-1}  cloaked={TF(window.IsCloaked),-1}  shell={TF(window.IsShellInfrastructure),-1}");

    Console.WriteLine(
        $"rect  : x={window.Bounds.X} y={window.Bounds.Y} w={window.Bounds.Width} h={window.Bounds.Height}");

    Console.WriteLine($"exe   : \"{Truncate(window.ExePath, 140)}\"");
    Console.WriteLine();
}

return;

static string FormatPid(int? processId) => processId.HasValue ? processId.Value.ToString() : "null";

static string FormatProcessName(string? exePath)
{
    if (string.IsNullOrWhiteSpace(exePath))
    {
        return "unknown";
    }

    return Path.GetFileName(exePath);
}

static string Truncate(string? value, int maxLength)
{
    var text = value ?? string.Empty;
    if (maxLength <= 3 || text.Length <= maxLength)
    {
        return text;
    }

    return text[..(maxLength - 3)] + "...";
}

static void PrintUsage()
{
    Console.WriteLine("WorkspaceApp.App window snapshot harness");
    Console.WriteLine("Usage: dotnet run --project src/WorkspaceApp.App [options]");
    Console.WriteLine("Options:");
    Console.WriteLine("  --limit <n>, -n <n>  Print at most n windows (n >= 0).");
    Console.WriteLine("  --include-hidden   Include non-visible windows.");
    Console.WriteLine("  --include-cloaked  Include DWM cloaked windows.");
    Console.WriteLine("  --include-shell    Include shell infrastructure windows.");
    Console.WriteLine("  -h, --help         Show this help.");
}

static string TF(bool b) => b ? "T" : "F";

static bool TryParseLimit(string[] args, out int? limit, out string? error)
{
    limit = null;
    error = null;

    for (var i = 0; i < args.Length; i++)
    {
        var isLimitOption =
            string.Equals(args[i], "--limit", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(args[i], "-n", StringComparison.OrdinalIgnoreCase);
        if (!isLimitOption)
        {
            continue;
        }

        if (i + 1 >= args.Length)
        {
            error = "Missing value for --limit.";
            return false;
        }

        var rawValue = args[i + 1];
        if (!int.TryParse(rawValue, out var parsedLimit) || parsedLimit < 0)
        {
            error = $"Invalid value for --limit: '{rawValue}'. Expected integer >= 0.";
            return false;
        }

        limit = parsedLimit;
        i++;
    }

    return true;
}
