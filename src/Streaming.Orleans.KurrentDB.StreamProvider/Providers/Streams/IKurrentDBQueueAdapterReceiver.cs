using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Statistics;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     The per-queue receiver and cache handed to the pulling agent. Each subscription strategy supplies its own, so
///     each only carries the logic its delivery and checkpoint model needs.
/// </summary>
public interface IKurrentDBQueueAdapterReceiver : IQueueAdapterReceiver, IQueueCache;

/// <summary>
///     Everything a subscription strategy needs to build the receiver for one queue.
/// </summary>
public sealed class KurrentDBQueueAdapterReceiverContext
{
    /// <summary>
    ///     The per-queue receiver settings.
    /// </summary>
    public required KurrentDBReceiverSettings Settings { get; init; }

    /// <summary>
    ///     Creates the message cache for the queue.
    /// </summary>
    public required Func<string, ILoggerFactory, IKurrentDBQueueCache> CacheFactory { get; init; }

    /// <summary>
    ///     Creates the KurrentDB receiver for the queue.
    /// </summary>
    public required Func<KurrentDBReceiverSettings, ILogger, IKurrentDBReceiver> ReceiverFactory { get; init; }

    /// <summary>
    ///     The logger factory.
    /// </summary>
    public required ILoggerFactory LoggerFactory { get; init; }

    /// <summary>
    ///     The receiver monitor for the queue.
    /// </summary>
    public required IQueueAdapterReceiverMonitor ReceiverMonitor { get; init; }

    /// <summary>
    ///     The load shedding options.
    /// </summary>
    public required LoadSheddingOptions LoadSheddingOptions { get; init; }

    /// <summary>
    ///     The environment statistics provider, when the host has one.
    /// </summary>
    public IEnvironmentStatisticsProvider? EnvironmentStatisticsProvider { get; init; }
}
