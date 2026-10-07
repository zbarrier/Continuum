namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
///     A KurrentDB stream name.
/// </summary>
public sealed class StreamName
{
    private const string AllStreamName = "$all";

    /// <summary>
    ///     The <c>$all</c> stream.
    /// </summary>
    public static readonly StreamName AllStream = new StreamName(AllStreamName);

    /// <summary>
    ///     Creates the name prefix of the streams in a category.
    /// </summary>
    /// <param name="streamCategory">The category.</param>
    /// <param name="prefix">An optional prefix, separated from the category by <c>__</c>.</param>
    public static StreamName ForCategory(string streamCategory, string? prefix = null)
        => prefix is null
        ? new StreamName($"{streamCategory}-")
        : new StreamName($"{prefix}__{streamCategory}-");

    /// <summary>
    ///     Creates the name of the stream a checkpoint is stored in.
    /// </summary>
    /// <param name="checkpointId">The checkpoint identifier.</param>
    /// <param name="prefix">An optional prefix, separated from the name by <c>__</c>.</param>
    public static StreamName ForCheckpoint(string checkpointId, string? prefix = null)
        => prefix is null
        ? new StreamName($"checkpoint-{checkpointId}")
        : new StreamName($"{prefix}__checkpoint-{checkpointId}");

    /// <summary>
    ///     Creates a stream name from an arbitrary value.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="prefix">An optional prefix, separated from the name by <c>__</c>.</param>
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

    /// <summary>
    ///     Whether this is the <c>$all</c> stream.
    /// </summary>
    public bool IsAllStream => _isAllStream ??= Value.Equals(AllStreamName, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    ///     Converts the stream name to its string value.
    /// </summary>
    /// <param name="self">The stream name.</param>
    public static implicit operator string(StreamName self) => self.Value;
}
