using Microsoft.Extensions.Logging;
using Orleans.Providers.Streams.Common;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Aggregated cache pressure monitor
/// </summary>
public class AggregatedCachePressureMonitor : List<ICachePressureMonitor>, ICachePressureMonitor
{
    private readonly ILogger _logger;
    private ICacheMonitor? _monitor;

    private bool _isUnderPressure;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AggregatedCachePressureMonitor" /> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="monitor"></param>
    public AggregatedCachePressureMonitor(ILogger logger, ICacheMonitor? monitor = null)
    {
        _isUnderPressure = false;
        _logger = logger;
        _monitor = monitor;
    }

    /// <summary>
    ///     Cache monitor which is used to report cache related metrics.
    /// </summary>
    public ICacheMonitor CacheMonitor
    {
        set => _monitor = value;
    }

    /// <summary>
    ///     Record cache pressure to every monitor in this aggregated cache monitor group.
    /// </summary>
    /// <param name="cachePressureContribution"></param>
    public void RecordCachePressureContribution(double cachePressureContribution)
    {
        foreach (var monitor in this)
        {
            monitor.RecordCachePressureContribution(cachePressureContribution);
        }
    }

    /// <summary>
    ///     If any monitor in this aggregated cache monitor group is under pressure, then return true.
    /// </summary>
    /// <param name="utcNow"></param>
    /// <returns></returns>
    public bool IsUnderPressure(DateTime utcNow)
    {
        var underPressure = false;
        foreach (var monitor in this)
        {
            if (monitor.IsUnderPressure(utcNow))
            {
                underPressure = true;
                break;
            }
        }
        if (_isUnderPressure != underPressure)
        {
            _isUnderPressure = underPressure;
            _monitor?.TrackCachePressureMonitorStatusChange(GetType().Name, _isUnderPressure, null, null, null);
            if (_isUnderPressure)
            {
                _logger.IngestionThrottled();
            }
            else
            {
                _logger.IngestionHealthy();
            }
        }
        return underPressure;
    }

    /// <summary>
    ///     Add one monitor to this aggregated cache monitor group.
    /// </summary>
    /// <param name="monitor"></param>
    public void AddCachePressureMonitor(ICachePressureMonitor monitor)
    {
        Add(monitor);
    }
}

internal static partial class AggregatedCachePressureMonitorLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Ingesting messages too fast. Throttling message reading.")]
    public static partial void IngestionThrottled(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Message ingestion is healthy.")]
    public static partial void IngestionHealthy(this ILogger logger);
}
