namespace WorkspaceApp.Domain.Operations;

/// <summary>
/// Represents a non-failure progress event emitted during an operation.
/// </summary>
public sealed class OperationEvent : OperationMessage
{
    public OperationEvent(
        DateTimeOffset timestampUtc,
        string code,
        string message,
        IEnumerable<KeyValuePair<string, string>>? data = null)
        : base(timestampUtc, code, message, data)
    {
    }
}
