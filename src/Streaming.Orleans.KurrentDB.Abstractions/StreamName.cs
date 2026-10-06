namespace Continuum.Streaming.Orleans.KurrentDB;

public sealed class StreamName
{
    private const string AllStreamName = "$all";

    public static readonly StreamName AllStream = new StreamName(AllStreamName);

    public static StreamName ForCategory(string streamCategory, string? prefix = null)
        => prefix is null
        ? new StreamName($"{streamCategory}-")
        : new StreamName($"{prefix}__{streamCategory}-");

    public static StreamName ForCheckpoint(string checkpointId, string? prefix = null)
        => prefix is null
        ? new StreamName($"checkpoint-{checkpointId}")
        : new StreamName($"{prefix}__checkpoint-{checkpointId}");

    public static StreamName Custom(string streamName, string? prefix = null)
        => prefix is null
        ? new StreamName(streamName)
        : new StreamName($"{prefix}__{streamName}");

    private StreamName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value));
        }
        Value = value;
    }

    private string Value { get; }

    private bool? _isAllStream = null;
    public bool IsAllStream => _isAllStream ??= Value.Equals(AllStreamName, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;

    public static implicit operator string(StreamName self) => self.Value;
}
