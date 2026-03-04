namespace WorkspaceApp.Domain.Operations;

public sealed class OperationResult
{
    public OperationResult(
        OperationStatus status,
        IEnumerable<OperationEvent>? events = null,
        IEnumerable<OperationError>? errors = null)
    {
        Status = status;
        Events = (events ?? Enumerable.Empty<OperationEvent>()).ToList().AsReadOnly();
        Errors = (errors ?? Enumerable.Empty<OperationError>()).ToList().AsReadOnly();
    }

    public OperationStatus Status { get; }

    public IReadOnlyList<OperationEvent> Events { get; }

    public IReadOnlyList<OperationError> Errors { get; }
}
