namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Implemented by batch containers that can report the <c>$all</c> commit position they were read at.
/// </summary>
/// <remarks>
///     The cache uses this to record how far each cursor has read, so purging, and therefore checkpointing, never
///     advances past an event a consumer has not been given yet.
/// </remarks>
public interface IKurrentDBCommitPositionBatch
{
    /// <summary>
    ///     The <c>$all</c> commit position this batch was read at.
    /// </summary>
    ulong CommitPosition { get; }
}
