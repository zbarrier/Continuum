using Microsoft.Extensions.Logging;

using Orleans.Providers.Streams.Common;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

/// <summary>
///     The age based <see cref="ChronologicalEvictionStrategy" /> used by the persistent subscription cache.
/// </summary>
/// <remarks>
///     Persistent subscriptions acknowledge events as they are received and keep their checkpoint on the server, so
///     purging never advances a checkpoint and does not need to wait for cursors.
/// </remarks>
internal sealed class KurrentDBPersistentEvictionStrategy : ChronologicalEvictionStrategy
{
    public KurrentDBPersistentEvictionStrategy(ILogger logger, TimePurgePredicate timePurge, ICacheMonitor? cacheMonitor, TimeSpan? monitorWriteInterval)
        : base(logger, timePurge, cacheMonitor, monitorWriteInterval)
    {
    }

    /// <summary>
    ///     Returns every block still held by the cache to its pool. Called when the cache is disposed, so blocks are not
    ///     stranded when a queue leaves this silo.
    /// </summary>
    public void ReleaseBlocks()
    {
        while (inUseBuffers.TryDequeue(out var block))
        {
            block.Dispose();
        }
    }
}
