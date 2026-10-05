using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.DependencyInjection;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Shared metrics and tracing for log consistent storage providers. Metric tags are kept low-cardinality
///     (no grain ids or stream names); those are recorded on spans only.
/// </summary>
internal sealed class LogConsistentStorageInstrumentation
{
    internal const string ReadOperation = "read";
    internal const string GetLastVersionOperation = "get_last_version";
    internal const string AppendOperation = "append";

    private static readonly ActivitySource ActivitySource = new(EventSourcingTelemetry.ActivitySourceName);
    private static readonly Lazy<Meter> FallbackMeter = new(() => new Meter(EventSourcingTelemetry.MeterName));

    private readonly string _dbSystem;
    private readonly string _providerName;
    private readonly Histogram<double> _duration;
    private readonly Counter<long> _events;
    private readonly Histogram<double>? _requestCharge;

    /// <param name="serviceProvider">Used to resolve <see cref="IMeterFactory"/>; a shared meter is used when none is registered.</param>
    /// <param name="dbSystem">The OpenTelemetry <c>db.system.name</c> value of the provider.</param>
    /// <param name="providerName">The name of the storage provider instance.</param>
    /// <param name="recordsRequestCharge">Whether the provider reports Cosmos DB request units.</param>
    internal LogConsistentStorageInstrumentation(IServiceProvider serviceProvider, string dbSystem, string providerName, bool recordsRequestCharge = false)
    {
        _dbSystem = dbSystem;
        _providerName = providerName;

        var meter = serviceProvider.GetService<IMeterFactory>()?.Create(EventSourcingTelemetry.MeterName) ?? FallbackMeter.Value;
        _duration = meter.CreateHistogram<double>("continuum.eventsourcing.operation.duration", unit: "s",
            description: "Duration of log consistent storage operations.");
        _events = meter.CreateCounter<long>("continuum.eventsourcing.events", unit: "{event}",
            description: "Number of events read from or appended to log consistent storage.");
        if (recordsRequestCharge)
        {
            _requestCharge = meter.CreateHistogram<double>("continuum.eventsourcing.cosmosdb.request_charge", unit: "{request_unit}",
                description: "Cosmos DB request units consumed by log consistent storage operations.");
        }
    }

    internal Operation Start(string operation, string grainTypeName, GrainId grainId, string streamName)
    {
        var activity = ActivitySource.StartActivity($"{operation} {grainTypeName}", ActivityKind.Client);
        if (activity is not null && activity.IsAllDataRequested)
        {
            activity.SetTag("db.system.name", _dbSystem);
            activity.SetTag("db.operation.name", operation);
            activity.SetTag("continuum.provider.name", _providerName);
            activity.SetTag("continuum.grain.type", grainTypeName);
            activity.SetTag("continuum.grain.id", grainId.ToString());
            activity.SetTag("continuum.stream.name", streamName);
        }
        return new Operation(this, activity, operation, grainTypeName);
    }

    internal sealed class Operation : IDisposable
    {
        private readonly LogConsistentStorageInstrumentation _owner;
        private readonly Activity? _activity;
        private readonly string _operation;
        private readonly string _grainTypeName;
        private readonly long _startTimestamp = Stopwatch.GetTimestamp();
        private int _eventCount;
        private double _requestCharge;
        private string? _errorType;
        private bool _disposed;

        internal Operation(LogConsistentStorageInstrumentation owner, Activity? activity, string operation, string grainTypeName)
        {
            _owner = owner;
            _activity = activity;
            _operation = operation;
            _grainTypeName = grainTypeName;
        }

        /// <summary>The request units consumed so far by this operation.</summary>
        internal double RequestCharge => _requestCharge;

        internal void SetEventCount(int eventCount) => _eventCount = eventCount;

        internal void AddRequestCharge(double requestCharge) => _requestCharge += requestCharge;

        internal void Fail(Exception exception)
        {
            _errorType = exception.GetType().FullName;
            _activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            _activity?.AddException(exception);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            var tags = new TagList
            {
                { "db.system.name", _owner._dbSystem },
                { "db.operation.name", _operation },
                { "continuum.provider.name", _owner._providerName },
                { "continuum.grain.type", _grainTypeName },
            };
            if (_errorType is not null)
            {
                tags.Add("error.type", _errorType);
            }

            _owner._duration.Record(Stopwatch.GetElapsedTime(_startTimestamp).TotalSeconds, tags);
            if (_eventCount > 0)
            {
                _owner._events.Add(_eventCount, tags);
            }
            if (_owner._requestCharge is not null)
            {
                _owner._requestCharge.Record(_requestCharge, tags);
            }

            if (_activity is not null)
            {
                _activity.SetTag("continuum.event.count", _eventCount);
                if (_owner._requestCharge is not null)
                {
                    _activity.SetTag("azure.cosmosdb.operation.request_charge", _requestCharge);
                }
                _activity.Dispose();
            }
        }
    }
}
