namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Stores the event log of a log-consistent grain.
/// </summary>
/// <remarks>
///     Every version is an event count, matching Orleans: 0 means no events have been written and 1 means one event
///     has been written. Implementations convert to store-specific positions (such as zero-based stream revisions)
///     internally and must not expose them.
/// </remarks>
public interface ILogConsistentStorage
{
    /// <summary>
    ///     Reads the log entries that follow a version.
    /// </summary>
    /// <remarks>
    ///     There are two equivalent ways to read <paramref name="fromVersion"/>:
    ///     <list type="bullet">
    ///         <item>As a version, it is exclusive: the grain is already at that version, so the first entry returned
    ///         is the one that produces version <paramref name="fromVersion"/> + 1.</item>
    ///         <item>As a zero-based position in the log, it is inclusive: the entry that produces version v is at
    ///         position v - 1, so the entry producing <paramref name="fromVersion"/> + 1 is at position
    ///         <paramref name="fromVersion"/>.</item>
    ///     </list>
    ///     That is why 0 reads from the beginning: version 0 is the empty log, and the first entry (position 0)
    ///     produces version 1.
    /// </remarks>
    /// <param name="grainTypeName">The grain type name.</param>
    /// <param name="grainId">The grain ID.</param>
    /// <param name="fromVersion">The version to read after (exclusive), which is also the zero-based position of the first entry to read (inclusive).</param>
    /// <param name="maxCount">The maximum number of entries to read.</param>
    /// <typeparam name="TLogEntry">The log entry type.</typeparam>
    /// <returns>The entries read, which is empty if the log does not exist.</returns>
    Task<IReadOnlyList<TLogEntry?>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int maxCount);

    /// <summary>
    ///     Gets the number of events in the log.
    /// </summary>
    /// <param name="grainTypeName">The grain type name.</param>
    /// <param name="grainId">The grain ID.</param>
    /// <returns>The event count, which is 0 if the log does not exist.</returns>
    Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId);

    /// <summary>
    ///     Appends entries
    /// </summary>
    /// <param name="grainTypeName">The grain type name.</param>
    /// <param name="grainId">The grain ID.</param>
    /// <param name="entries">The entries to append atomically.</param>
    /// <param name="expectedVersion">The event count the log must currently have; 0 means the log must not exist.</param>
    /// <typeparam name="TLogEntry">The log entry type.</typeparam>
    /// <returns>The event count after the append.</returns>
    /// <exception cref="global::Orleans.Storage.InconsistentStateException">The log does not have <paramref name="expectedVersion"/> events.</exception>
    Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion);
}
