namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Aggregation dimensions for block pool monitor used in KurrentDB stream provider ecosystem
/// </summary>
public class KurrentDBBlockPoolMonitorDimensions : KurrentDBMonitorAggregationDimensions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBBlockPoolMonitorDimensions" /> class.
    /// </summary>
    public KurrentDBBlockPoolMonitorDimensions()
    {
        BlockPoolId = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBBlockPoolMonitorDimensions" /> class.
    /// </summary>
    /// <param name="dimensions"></param>
    /// <param name="blockPoolId"></param>
    public KurrentDBBlockPoolMonitorDimensions(KurrentDBMonitorAggregationDimensions dimensions, string blockPoolId)
        : base(dimensions)
    {
        BlockPoolId = blockPoolId;
    }

    /// <summary>
    ///     Block pool Id.
    /// </summary>
    public string BlockPoolId { get; set; }
}
