using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     A queue cache for catch-up subscriptions, which tracks how far each cursor has read so purging, and therefore
///     the client-side <c>$all</c> checkpoint, never advances past an event a consumer has not received.
/// </summary>
public interface IKurrentDBCatchupQueueCache : IKurrentDBQueueCache
{
    /// <summary>
    ///     Stop tracking a cursor that is no longer reading, so it no longer holds back purging.
    /// </summary>
    /// <param name="cursorObj"></param>
    void ReleaseCursor(object cursorObj);

    /// <summary>
    ///     Records that a cursor has finished with the event at <paramref name="commitPosition" />, so the cache may
    ///     purge, and therefore checkpoint, past it.
    /// </summary>
    /// <remarks>
    ///     Called once the event has been delivered rather than when it was handed out, so an event that failed to
    ///     deliver keeps holding the watermark down and is still available after a restart.
    /// </remarks>
    /// <param name="cursorObj"></param>
    /// <param name="commitPosition"></param>
    void RecordDelivered(object cursorObj, ulong commitPosition);

    /// <summary>
    ///     Records that a cursor has no pending events for its stream, so it has read everything currently cached that it
    ///     could ever need and no longer holds back purging of other streams.
    /// </summary>
    /// <param name="cursorObj"></param>
    void RecordCaughtUp(object cursorObj);

    /// <summary>
    ///     Raised with the <c>$all</c> commit position of the newest event that has been purged, and is therefore
    ///     behind every active cursor.
    /// </summary>
    Action<ulong>? OnCheckpointablePosition { get; set; }

    /// <summary>
    ///     The transaction still being read, or <see langword="null" /> when none is.
    /// </summary>
    PendingTransaction? PendingTransaction { get; }

    /// <summary>
    ///     Handles the transaction still being read as partial, once KurrentDB has confirmed no more of it will arrive,
    ///     according to the configured <see cref="Continuum.Streaming.Orleans.KurrentDB.Configuration.PartialTransactionHandling" />.
    /// </summary>
    /// <returns>The positions cached, or an empty list when no transaction was pending.</returns>
    /// <exception cref="KurrentDBPartialTransactionException">The handling is to halt.</exception>
    List<StreamPosition> ReleasePartialTransaction(DateTime utcNow);
}

/// <summary>
///     A transaction a catch-up cache is still reading.
/// </summary>
/// <param name="TransactionId">The transaction.</param>
/// <param name="LastPosition">The position of the last event received of it.</param>
/// <param name="LastReceivedUtc">When the last event of it was received.</param>
public readonly record struct PendingTransaction(Guid TransactionId, ulong LastPosition, DateTime LastReceivedUtc);
