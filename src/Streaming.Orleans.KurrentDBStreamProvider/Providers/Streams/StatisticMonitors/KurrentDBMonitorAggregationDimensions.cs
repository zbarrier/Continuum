namespace Orleans.Providers.Streams.KurrentDB.StatisticMonitors;

/// <summary>
///     Base class for monitor aggregation dimensions, which is an information bag for the monitoring target.
///     Monitors can use this information bag to build its aggregation dimensions.
/// </summary>
public class KurrentDBMonitorAggregationDimensions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBMonitorAggregationDimensions" /> class.
    /// </summary>
    public KurrentDBMonitorAggregationDimensions()
    {
        Name = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBMonitorAggregationDimensions" /> class.
    /// </summary>
    /// <param name="name"></param>
    public KurrentDBMonitorAggregationDimensions(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBMonitorAggregationDimensions" /> class.
    /// </summary>
    /// <param name="dimensions"></param>
    public KurrentDBMonitorAggregationDimensions(KurrentDBMonitorAggregationDimensions dimensions)
    {
        Name = dimensions.Name;
    }

    /// <summary>
    ///     KurrentDB name.
    /// </summary>
    public string Name { get; set; }
}
