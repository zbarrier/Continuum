using Microsoft.Extensions.Logging;

using Orleans.Providers.Streams.Common;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Everything a subscription strategy needs to build the message cache for one queue.
/// </summary>
public sealed class KurrentDBQueueCacheContext
{
    /// <summary>
    ///     The queue name.
    /// </summary>
    public required string Queue { get; init; }

    /// <summary>
    ///     The data adapter that converts queue messages into cached messages.
    /// </summary>
    public required IKurrentDBDataAdapter DataAdapter { get; init; }

    /// <summary>
    ///     The pool the cache allocates its blocks from.
    /// </summary>
    public required IObjectPool<FixedSizeBuffer> BufferPool { get; init; }

    /// <summary>
    ///     Decides when cached messages are old enough to purge.
    /// </summary>
    public required TimePurgePredicate TimePurge { get; init; }

    /// <summary>
    ///     The cache monitor.
    /// </summary>
    public required ICacheMonitor CacheMonitor { get; init; }

    /// <summary>
    ///     The logger factory the cache creates its logger from.
    /// </summary>
    public required ILoggerFactory LoggerFactory { get; init; }

    /// <summary>
    ///     Appended to the cache type's full name to form the cache logger's category.
    /// </summary>
    public required string LoggerNameSuffix { get; init; }

    /// <summary>
    ///     Creates the cache logger, named after the cache type.
    /// </summary>
    public ILogger CreateLogger<TCache>() => LoggerFactory.CreateLogger($"{typeof(TCache).FullName}.{LoggerNameSuffix}");

    /// <summary>
    ///     How often cache statistics are reported.
    /// </summary>
    public TimeSpan? StatisticMonitorWriteInterval { get; init; }

    /// <summary>
    ///     How long stream metadata stays in the cache.
    /// </summary>
    public TimeSpan? MetadataMinTimeInCache { get; init; }
}
