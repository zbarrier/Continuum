using Orleans.Providers.Streams.Common;
using Orleans.Runtime;

namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Default cache monitor for KurrentDB streaming provider ecosystem
/// </summary>
public class DefaultKurrentDBCacheMonitor : DefaultCacheMonitor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultKurrentDBCacheMonitor" /> class.
    /// </summary>
    /// <param name="dimensions">Aggregation Dimension bag for KurrentDBCacheMonitor</param>
    /// <param name="instruments">The Orleans instruments used to publish metrics.</param>
    public DefaultKurrentDBCacheMonitor(KurrentDBCacheMonitorDimensions dimensions, OrleansInstruments instruments)
        : base(new KeyValuePair<string, object>[]
               {
                   new("Path", dimensions.Name),
                   new("Stream", dimensions.QueueName)
               }, instruments)
    {
    }
}
