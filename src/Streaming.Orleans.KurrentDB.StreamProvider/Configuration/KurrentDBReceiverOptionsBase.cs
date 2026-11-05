namespace Orleans.Configuration;

/// <summary>
///     Options common to every KurrentDB receiver implementation, regardless of the subscription strategy used.
/// </summary>
/// <remarks>
///     Concrete strategies derive from this type: <see cref="KurrentDBPersistentSubscriptionReceiverOptions" /> for
///     server managed consumer groups and <see cref="KurrentDBCatchupSubscriptionReceiverOptions" /> for a client checkpointed
///     <c>$all</c> subscription. The strategy is selected through the stream configurator, which registers only the
///     options class that applies, so that start up validation cannot fail on options belonging to the other strategy.
///     Deserialization is not configured here; receivers hand raw event records to the queue data adapter, which owns
///     the serializer and type mapper through <see cref="KurrentDBDataAdapterOptions" />.
/// </remarks>
public abstract class KurrentDBReceiverOptionsBase
{
    /// <summary>
    ///     In cases where no checkpoint is found, this indicates if the service should read from the most recent data,
    ///     or from the beginning of a stream.
    /// </summary>
    /// <remarks>
    ///     A catch-up subscription commits its starting position right away, so a restart before the first regular
    ///     checkpoint does not choose a new one. A single stream subscription whose stream does not exist yet commits a
    ///     position before the stream's first event. Defaults to <see langword="false" /> for catch-up subscriptions, so a
    ///     subscription with no checkpoint reads its full history, and to <see langword="true" /> for persistent
    ///     subscriptions.
    /// </remarks>
    public bool StartFromNow { get; set; }
}
