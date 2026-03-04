using System.Collections.ObjectModel;

namespace WorkspaceApp.Domain.Operations;

public abstract class OperationMessage
{
    protected OperationMessage(
        DateTimeOffset timestampUtc,
        string code,
        string message,
        IEnumerable<KeyValuePair<string, string>>? data = null)
    {
        if (timestampUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("TimestampUtc must be in UTC.", nameof(timestampUtc));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        }

        TimestampUtc = timestampUtc;
        Code = code.Trim();
        Message = message.Trim();

        var dataMap = (data ?? Enumerable.Empty<KeyValuePair<string, string>>())
            .ToDictionary(static kv => kv.Key, static kv => kv.Value, StringComparer.Ordinal);
        Data = new ReadOnlyDictionary<string, string>(dataMap);
    }

    public DateTimeOffset TimestampUtc { get; }

    public string Code { get; }

    public string Message { get; }

    public IReadOnlyDictionary<string, string> Data { get; }
}
