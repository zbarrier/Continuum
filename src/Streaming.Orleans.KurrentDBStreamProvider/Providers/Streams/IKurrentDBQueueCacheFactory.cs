using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Factory responsible for creating a message cache for a KurrentDB queue.
/// </summary>
public interface IKurrentDBQueueCacheFactory
{
    /// <summary>
    ///     Function used to create a IKurrentDBQueueCache
    /// </summary>
    IKurrentDBQueueCache CreateCache(string queue, IStreamQueueCheckpointer<string> checkpointer, ILoggerFactory loggerFactory);
}
