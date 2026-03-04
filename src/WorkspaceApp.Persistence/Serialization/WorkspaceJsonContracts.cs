using System.Text.Json;
using System.Text.Json.Serialization;
using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence.Serialization;

public sealed class WorkspaceDocument
{
    [JsonPropertyName("id")]
    public required WorkspaceId Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; init; }

    [JsonPropertyName("updatedAtUtc")]
    public DateTimeOffset UpdatedAtUtc { get; init; }

    [JsonPropertyName("entries")]
    public List<ApplicationEntryDocument> Entries { get; init; } = [];

    [JsonPropertyName("schemaVersion")]
    public SchemaVersion SchemaVersion { get; init; }

    [JsonExtensionData]
    // Preserve unknown fields so newer schema data is not dropped by older binaries.
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

public sealed class ApplicationEntryDocument
{
    [JsonPropertyName("exePath")]
    public required string ExePath { get; init; }

    [JsonPropertyName("windows")]
    public List<WindowLayoutDocument> Windows { get; init; } = [];
}

public sealed class WindowLayoutDocument
{
    [JsonPropertyName("bounds")]
    public required RectDocument Bounds { get; init; }

    [JsonPropertyName("zOrderIndex")]
    public int ZOrderIndex { get; init; }

    [JsonPropertyName("monitorHint")]
    public MonitorHintDocument? MonitorHint { get; init; }

    [JsonPropertyName("titleHint")]
    public string? TitleHint { get; init; }
}

public sealed class RectDocument
{
    [JsonPropertyName("x")]
    public int X { get; init; }

    [JsonPropertyName("y")]
    public int Y { get; init; }

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }
}

public sealed class MonitorHintDocument
{
    [JsonPropertyName("monitorIndex")]
    public int? MonitorIndex { get; init; }
}
