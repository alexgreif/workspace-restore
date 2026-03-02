namespace WorkspaceApp.Domain.Models;

public readonly record struct WorkspaceId
{
    public WorkspaceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "WorkspaceId cannot be null or whitespace.",
                nameof(value)
            );
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static WorkspaceId New() => new(Guid.NewGuid().ToString("D"));
}
