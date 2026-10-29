using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Creates the <see cref="IKurrentDBReceiver" /> for the subscription strategy selected on a stream provider,
///     and exposes the receiver options belonging to that strategy.
/// </summary>
/// <remarks>
///     One implementation is registered as a keyed service per stream provider name by the
///     <c>UsePersistentSubscriptions</c>/<c>UseCatchUpSubscription</c> configurator extensions. This keeps the
///     strategy selection out of the queue adapter factory and ensures only the selected strategy's options are
///     resolved and validated.
/// </remarks>
public interface IKurrentDBReceiverFactory
{
    /// <summary>
    ///     The receiver options for the selected strategy.
    /// </summary>
    KurrentDBReceiverOptionsBase ReceiverOptions { get; }

    /// <summary>
    ///     Whether the client owns the subscription checkpoint, so the queue cache must track cursors and only purge,
    ///     and therefore checkpoint, events every cursor has finished with.
    /// </summary>
    bool ClientCheckpointed { get; }

    /// <summary>
    ///     Whether the stream provider may write events. Catch-up subscriptions read streams owned by event sourced
    ///     grains, so they are read only; writing would bypass the grain that owns the stream.
    /// </summary>
    StreamProviderDirection Direction { get; }

    /// <summary>
    ///     Creates a receiver for a single queue.
    /// </summary>
    /// <param name="settings">The per-queue receiver settings.</param>
    /// <param name="logger">The logger.</param>
    IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, ILogger logger);

    /// <summary>
    ///     Creates the receiver and cache the pulling agent uses for a single queue.
    /// </summary>
    /// <param name="context">What the queue adapter supplies for the queue.</param>
    IKurrentDBQueueAdapterReceiver CreateQueueAdapterReceiver(KurrentDBQueueAdapterReceiverContext context);

    /// <summary>
    ///     Creates the producer that appends events to a single queue stream. Only called when
    ///     <see cref="Direction" /> allows writes.
    /// </summary>
    /// <param name="queueName">The KurrentDB stream backing the queue.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <exception cref="NotSupportedException">The strategy is read only.</exception>
    IKurrentDBProducer CreateProducer(string queueName, ILoggerFactory loggerFactory);
}
