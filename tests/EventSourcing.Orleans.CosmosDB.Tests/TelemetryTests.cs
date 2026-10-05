using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

[Collection(ClusterCollection.Name)]
public sealed class TelemetryTests(ClusterFixture fixture) : IDisposable
{
    private readonly TelemetryCollector _telemetry = new();

    // A unique grain type per test isolates its metrics from other storage calls.
    private readonly string _grainTypeName = $"telemetry{Guid.NewGuid():N}";

    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private ILogConsistentStorage Storage => SiloServices.GetRequiredService<ILogConsistentStorage>();

    public void Dispose() => _telemetry.Dispose();

    private GrainId NewGrainId() => GrainId.Create(_grainTypeName, Guid.NewGuid().ToString("N"));

    private static SnackEvent[] CreateEvents(int count)
    {
        var id = Guid.NewGuid();
        var events = new SnackEvent[count];
        events[0] = new SnackInitializedEvent(id, "Name0", Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", 1);
        for (var index = 1; index < count; index++)
        {
            events[index] = new SnackNameChangedEvent(id, $"Name{index}", Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", index + 1);
        }
        return events;
    }

    [Fact]
    public async Task Should_Record_Metrics_And_Spans_For_Storage_Operations()
    {
        var grainId = NewGrainId();

        await Storage.AppendAsync(_grainTypeName, grainId, CreateEvents(3), 0);
        await Storage.ReadAsync<SnackEvent>(_grainTypeName, grainId, 0, 10);
        await Storage.GetLastVersionAsync(_grainTypeName, grainId);

        foreach (var operation in new[] { "append", "read", "get_last_version" })
        {
            var span = Assert.Single(_telemetry.SpansFor(grainId), s => (string?)s.GetTagItem("db.operation.name") == operation);
            Assert.Equal("azure.cosmosdb", span.GetTagItem("db.system.name"));
            Assert.Equal(ActivityKind.Client, span.Kind);
            Assert.NotEqual(ActivityStatusCode.Error, span.Status);
            Assert.NotNull(span.GetTagItem("continuum.stream.name"));
            Assert.True((double)span.GetTagItem("azure.cosmosdb.operation.request_charge")! > 0);

            var duration = Assert.Single(_telemetry.MeasurementsFor(_grainTypeName, "continuum.eventsourcing.operation.duration", operation));
            Assert.True(duration.Value >= 0);
            Assert.Equal("azure.cosmosdb", duration.Tags["db.system.name"]);
            Assert.NotNull(duration.Tags["continuum.provider.name"]);
            Assert.DoesNotContain("continuum.grain.id", duration.Tags.Keys);
            Assert.DoesNotContain("continuum.stream.name", duration.Tags.Keys);
            Assert.DoesNotContain("error.type", duration.Tags.Keys);

            var requestCharge = Assert.Single(_telemetry.MeasurementsFor(_grainTypeName, "continuum.eventsourcing.cosmosdb.request_charge", operation));
            Assert.True(requestCharge.Value > 0);
        }

        Assert.Equal(3, Assert.Single(_telemetry.MeasurementsFor(_grainTypeName, "continuum.eventsourcing.events", "append")).Value);
        Assert.Equal(3, Assert.Single(_telemetry.MeasurementsFor(_grainTypeName, "continuum.eventsourcing.events", "read")).Value);
        Assert.Empty(_telemetry.MeasurementsFor(_grainTypeName, "continuum.eventsourcing.events", "get_last_version"));
    }

    [Fact]
    public async Task Should_Mark_Failed_Operations_As_Errors()
    {
        var grainId = NewGrainId();

        await Assert.ThrowsAsync<InconsistentStateException>(() => Storage.AppendAsync(_grainTypeName, grainId, CreateEvents(1), 1));

        var span = Assert.Single(_telemetry.SpansFor(grainId));
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        var duration = Assert.Single(_telemetry.MeasurementsFor(_grainTypeName, "continuum.eventsourcing.operation.duration", "append"));
        Assert.Equal(typeof(InconsistentStateException).FullName, duration.Tags["error.type"]);
    }

    [Theory]
    [InlineData(0, LogLevel.Warning)]
    [InlineData(1_000_000, LogLevel.Debug)]
    public async Task Should_Log_Request_Charge_Based_On_Warning_Threshold(double threshold, LogLevel expectedLevel)
    {
        var logger = new CapturingLogger();
        var storage = await StartStorageAsync(threshold, logger);
        var grainId = NewGrainId();

        await storage.AppendAsync(_grainTypeName, grainId, CreateEvents(2), 0);

        var entry = Assert.Single(logger.Entries, e => e.Message.Contains("RUs", StringComparison.Ordinal));
        Assert.Equal(expectedLevel, entry.Level);
        Assert.Contains("'append'", entry.Message);
    }

    // Creates a provider instance sharing the silo's client, serializer and type mapper but with its own threshold and logger.
    private async Task<CosmosDBLogConsistentStorage> StartStorageAsync(double requestChargeWarningThreshold, ILogger<CosmosDBLogConsistentStorage> logger)
    {
        var siloOptions = SiloServices.GetRequiredService<IOptionsMonitor<CosmosDBLogConsistentStorageOptions>>()
            .Get(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME);
        var options = new CosmosDBLogConsistentStorageOptions
        {
            ConnectionName = siloOptions.ConnectionName,
            DatabaseName = siloOptions.DatabaseName,
            ContainerName = siloOptions.ContainerName,
            GrainStorageSerializer = siloOptions.GrainStorageSerializer,
            TypeMapper = siloOptions.TypeMapper,
            RequestChargeWarningThreshold = requestChargeWarningThreshold,
        };
        var storage = new CosmosDBLogConsistentStorage(SiloServices, "telemetry", options,
            SiloServices.GetRequiredService<IOptions<ClusterOptions>>(), logger);

        var lifecycle = new CapturingLifecycle();
        storage.Participate(lifecycle);
        foreach (var start in lifecycle.OnStart)
        {
            await start(TestContext.Current.CancellationToken);
        }
        return storage;
    }

    private sealed record Measurement(string Instrument, double Value, IReadOnlyDictionary<string, object?> Tags);

    private sealed class TelemetryCollector : IDisposable
    {
        private readonly MeterListener _meterListener = new();
        private readonly ActivityListener _activityListener;
        private readonly ConcurrentQueue<Measurement> _measurements = new();
        private readonly ConcurrentQueue<Activity> _spans = new();

        public TelemetryCollector()
        {
            _meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == EventSourcingTelemetry.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            _meterListener.SetMeasurementEventCallback<double>((instrument, value, tags, _) => Record(instrument, value, tags));
            _meterListener.SetMeasurementEventCallback<long>((instrument, value, tags, _) => Record(instrument, value, tags));
            _meterListener.Start();

            _activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == EventSourcingTelemetry.ActivitySourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = _spans.Enqueue,
            };
            ActivitySource.AddActivityListener(_activityListener);
        }

        private void Record(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            var tagDictionary = new Dictionary<string, object?>();
            foreach (var tag in tags)
            {
                tagDictionary[tag.Key] = tag.Value;
            }
            _measurements.Enqueue(new Measurement(instrument.Name, value, tagDictionary));
        }

        public IEnumerable<Activity> SpansFor(GrainId grainId)
            => _spans.Where(s => (string?)s.GetTagItem("continuum.grain.id") == grainId.ToString());

        public IEnumerable<Measurement> MeasurementsFor(string grainTypeName, string instrument, string operation)
            => _measurements.Where(m => m.Instrument == instrument
                && Equals(m.Tags.GetValueOrDefault("continuum.grain.type"), grainTypeName)
                && Equals(m.Tags.GetValueOrDefault("db.operation.name"), operation));

        public void Dispose()
        {
            _meterListener.Dispose();
            _activityListener.Dispose();
        }
    }

    private sealed class CapturingLogger : ILogger<CosmosDBLogConsistentStorage>
    {
        public ConcurrentQueue<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Enqueue((logLevel, formatter(state, exception)));
    }

    private sealed class CapturingLifecycle : ISiloLifecycle
    {
        public List<Func<CancellationToken, Task>> OnStart { get; } = [];

        public int HighestCompletedStage => 0;

        public int LowestStoppedStage => 0;

        public IDisposable Subscribe(string observerName, int stage, ILifecycleObserver observer)
        {
            OnStart.Add(observer.OnStart);
            return new NoopDisposable();
        }

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
