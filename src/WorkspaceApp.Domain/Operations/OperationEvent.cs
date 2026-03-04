namespace WorkspaceApp.Domain.Operations;

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
