using System.Text.Json;
using System.Text.Json.Serialization;
using WorkspaceApp.Domain.Models;

namespace WorkspaceApp.Persistence.Serialization;

// Enforces a stable persisted representation for WorkspaceId as a JSON string.
public sealed class WorkspaceIdJsonConverter : JsonConverter<WorkspaceId>
{
    public override WorkspaceId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("WorkspaceId must be a JSON string.");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("WorkspaceId cannot be empty.");
        }

        return new WorkspaceId(value);
    }

    public override void Write(Utf8JsonWriter writer, WorkspaceId value, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(value.Value))
        {
            throw new JsonException("WorkspaceId cannot be empty.");
        }

        writer.WriteStringValue(value.Value);
    }
}
