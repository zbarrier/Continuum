namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Aggregation dimensions for KurrentDBReceiverMonitor
/// </summary>
public class KurrentDBReceiverMonitorDimensions : KurrentDBMonitorAggregationDimensions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBReceiverMonitorDimensions" /> class.
    /// </summary>
    public KurrentDBReceiverMonitorDimensions()
    {
        QueueName = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBReceiverMonitorDimensions" /> class.
    /// </summary>
    /// <param name="dimensions"></param>
    /// <param name="queue"></param>
    public KurrentDBReceiverMonitorDimensions(KurrentDBMonitorAggregationDimensions dimensions, string queue)
        : base(dimensions)
    {
        QueueName = queue;
    }

    /// <summary>
    ///     KurrentDB stream name (Aka queue name)
    /// </summary>
    public string QueueName { get; set; }
}
