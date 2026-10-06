using Orleans.Providers.Streams.Common;
using Orleans.Runtime;

namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Default monitor for Object pool used by KurrentDBStreamProvider
/// </summary>
public class DefaultKurrentDBBlockPoolMonitor : DefaultBlockPoolMonitor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultKurrentDBBlockPoolMonitor" /> class.
    /// </summary>
    /// <param name="dimensions">Aggregation Dimension bag for KurrentDBBlockPoolMonitor</param>
    /// <param name="instruments">The Orleans instruments used to publish metrics.</param>
    public DefaultKurrentDBBlockPoolMonitor(KurrentDBBlockPoolMonitorDimensions dimensions, OrleansInstruments instruments)
        : base(new KeyValuePair<string, object>[]
               {
                   new("Path", dimensions.Name),
                   new("ObjectPoolId", dimensions.BlockPoolId)
               }, instruments)
    {
    }
}
