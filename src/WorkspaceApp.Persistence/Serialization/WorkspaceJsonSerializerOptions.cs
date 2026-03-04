using System.Text.Json;

namespace WorkspaceApp.Persistence.Serialization;

public static class WorkspaceJsonSerializerOptions
{
    public static JsonSerializerOptions Default { get; } = CreateDefault();

    private static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            WriteIndented = true
        };

        options.Converters.Add(new WorkspaceIdJsonConverter());
        return options;
    }
}
