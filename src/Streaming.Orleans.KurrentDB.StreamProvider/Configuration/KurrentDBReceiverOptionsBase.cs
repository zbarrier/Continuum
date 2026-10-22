namespace Orleans.Configuration;

/// <summary>
///     Options common to every KurrentDB receiver implementation, regardless of the subscription strategy used.
/// </summary>
/// <remarks>
///     Concrete strategies derive from this type: <see cref="KurrentDBPersistentSubscriptionReceiverOptions" /> for
///     server managed consumer groups and <see cref="KurrentDBAllStreamReceiverOptions" /> for a client checkpointed
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
    public bool StartFromNow { get; set; } = true;
}
