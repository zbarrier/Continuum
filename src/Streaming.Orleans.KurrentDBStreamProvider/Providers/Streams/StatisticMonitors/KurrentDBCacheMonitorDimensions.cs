namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Aggregation dimensions for cache monitor used in KurrentDB stream provider ecosystem
/// </summary>
public class KurrentDBCacheMonitorDimensions : KurrentDBReceiverMonitorDimensions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCacheMonitorDimensions" /> class.
    /// </summary>
    public KurrentDBCacheMonitorDimensions()
    {
        BlockPoolId = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCacheMonitorDimensions" /> class.
    /// </summary>
    /// <param name="dimensions"></param>
    /// <param name="queue"></param>
    /// <param name="blockPoolId"></param>
    public KurrentDBCacheMonitorDimensions(KurrentDBMonitorAggregationDimensions dimensions, string queue, string blockPoolId)
        : base(dimensions, queue)
    {
        BlockPoolId = blockPoolId;
    }

    /// <summary>
    ///     Block pool this cache belongs to.
    /// </summary>
    public string BlockPoolId { get; set; }

}
