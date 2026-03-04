namespace WorkspaceApp.Domain.Operations;

public sealed class OperationError : OperationMessage
{
    public OperationError(
        DateTimeOffset timestampUtc,
        string code,
        string message,
        IEnumerable<KeyValuePair<string, string>>? data = null)
        : base(timestampUtc, code, message, data)
    {
    }
}
