using Continuum.Streaming.Orleans;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Abstraction on KurrentDBReceiver class, used to configure KurrentDBReceiver class in KurrentDBQueueAdapterReceiver,
///     also used to configure KurrentDBGeneratorReceiver in KurrentDBQueueAdapterReceiver for testing purpose
/// </summary>
public interface IKurrentDBReceiver
{
    /// <summary>
    ///     Start to create client and subscribe from KurrentDB persistent subscriptions.
    /// </summary>
    Task InitAsync();

    /// <summary>
    ///     Clean up.
    /// </summary>
    Task CloseAsync();

    /// <summary>
    ///     Asking for more messages from internal queue.
    /// </summary>
    /// <param name="maxCount">Max amount of message which should be delivered</param>
    /// <returns>
    ///     The events read from the queue. The payload is left undeserialized so that it can be copied straight into
    ///     the cache buffers and materialized only when a consumer reads it.
    /// </returns>
    /// <remarks>
    ///     <see cref="StreamedEvent{T}.SequenceNumber" /> carries the position the checkpoint is written from. For the
    ///     <c>$all</c> strategy this is the global commit position, which cannot be recovered from the event record
    ///     once link resolution is enabled, so it must be captured here rather than further down the pipeline.
    /// </remarks>
    List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount);

    /// <summary>
    ///     Notifies the receiver that every event up to <paramref name="commitPosition" /> has been delivered to all
    ///     consumers and purged from the cache, so it is safe to checkpoint.
    /// </summary>
    /// <param name="commitPosition">The position, in the receiver's own checkpoint space, that has been fully handled.</param>
    /// <remarks>
    ///     Checkpointing is driven from the cache rather than from <see cref="Receive" />, because an event that has
    ///     only been buffered has not necessarily reached a consumer yet, and checkpointing it would lose it if the
    ///     silo stopped before delivery. Receivers whose broker owns the acknowledgement, such as persistent
    ///     subscriptions, may ignore this.
    /// </remarks>
    Task MessagesDeliveredAsync(ulong commitPosition);
}
