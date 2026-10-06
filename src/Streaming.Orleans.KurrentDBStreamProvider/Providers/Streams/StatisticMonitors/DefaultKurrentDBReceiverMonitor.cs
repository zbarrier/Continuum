using Orleans.Providers.Streams.Common;
using Orleans.Runtime;

namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Default KurrentDB receiver monitor that tracks metrics using loggers PKI support.
/// </summary>
public class DefaultKurrentDBReceiverMonitor : DefaultQueueAdapterReceiverMonitor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultKurrentDBReceiverMonitor" /> class.
    /// </summary>
    /// <param name="dimensions">Aggregation Dimension bag for KurrentDBReceiverMonitor</param>
    /// <param name="instruments">The Orleans instruments used to publish metrics.</param>
    public DefaultKurrentDBReceiverMonitor(KurrentDBReceiverMonitorDimensions dimensions, OrleansInstruments instruments)
        : base(new KeyValuePair<string, object>[]
               {
                   new("Path", dimensions.Name),
                   new("Stream", dimensions.QueueName)
               }, instruments)
    {
    }
}
