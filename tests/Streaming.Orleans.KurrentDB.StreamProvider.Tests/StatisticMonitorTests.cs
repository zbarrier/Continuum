using System.Diagnostics.Metrics;

using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.StatisticMonitors;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
/// Verifies the monitor dimension bags copy their values and that the default monitors publish metrics tagged with
/// those dimensions.
/// </summary>
public class StatisticMonitorTests
{
    private static readonly OrleansInstruments Instruments = new(new ServiceCollection().AddMetrics().BuildServiceProvider().GetRequiredService<IMeterFactory>());

    private sealed class TagListener : IDisposable
    {
        private readonly MeterListener _listener = new();
        private readonly string _path;

        public TagListener(string path)
        {
            _path = path;
            _listener.InstrumentPublished = (instrument, listener) => listener.EnableMeasurementEvents(instrument);
            _listener.SetMeasurementEventCallback<long>((_, _, tags, _) => Record(tags));
            _listener.SetMeasurementEventCallback<int>((_, _, tags, _) => Record(tags));
            _listener.Start();
        }

        public List<Dictionary<string, object?>> Measurements { get; } = [];

        public void Collect() => _listener.RecordObservableInstruments();

        private void Record(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            var dictionary = new Dictionary<string, object?>();
            foreach (var tag in tags)
            {
                dictionary[tag.Key] = tag.Value;
            }

            if (Equals(dictionary.GetValueOrDefault("Path"), _path))
            {
                lock (Measurements)
                {
                    Measurements.Add(dictionary);
                }
            }
        }

        public void Dispose() => _listener.Dispose();
    }

    [Fact]
    public void Dimensions_copy_the_shared_name_and_their_own_values()
    {
        var shared = new KurrentDBMonitorAggregationDimensions("provider");

        var copy = new KurrentDBMonitorAggregationDimensions(shared);
        var receiver = new KurrentDBReceiverMonitorDimensions(shared, "queue-1");
        var cache = new KurrentDBCacheMonitorDimensions(shared, "queue-2", "pool-1");
        var blockPool = new KurrentDBBlockPoolMonitorDimensions(shared, "pool-2");

        Assert.Equal("provider", copy.Name);
        Assert.Equal(("provider", "queue-1"), (receiver.Name, receiver.QueueName));
        Assert.Equal(("provider", "queue-2", "pool-1"), (cache.Name, cache.QueueName, cache.BlockPoolId));
        Assert.Equal(("provider", "pool-2"), (blockPool.Name, blockPool.BlockPoolId));
    }

    [Fact]
    public void Receiver_monitor_tags_metrics_with_path_and_stream()
    {
        var path = Guid.NewGuid().ToString();
        using var listener = new TagListener(path);
        var monitor = new DefaultKurrentDBReceiverMonitor(new KurrentDBReceiverMonitorDimensions(new(path), "queue-1"), Instruments);

        monitor.TrackMessagesReceived(3, null, null);

        listener.Collect();
        Assert.NotEmpty(listener.Measurements);
        Assert.All(listener.Measurements, tags => Assert.Equal("queue-1", tags["Stream"]));
    }

    [Fact]
    public void Cache_monitor_tags_metrics_with_path_and_stream()
    {
        var path = Guid.NewGuid().ToString();
        using var listener = new TagListener(path);
        var monitor = new DefaultKurrentDBCacheMonitor(new KurrentDBCacheMonitorDimensions(new(path), "queue-1", "pool-1"), Instruments);

        monitor.TrackMessagesAdded(4);

        listener.Collect();
        Assert.NotEmpty(listener.Measurements);
        Assert.All(listener.Measurements, tags => Assert.Equal("queue-1", tags["Stream"]));
    }

    [Fact]
    public void Block_pool_monitor_tags_metrics_with_path_and_pool_id()
    {
        var path = Guid.NewGuid().ToString();
        using var listener = new TagListener(path);
        var monitor = new DefaultKurrentDBBlockPoolMonitor(new KurrentDBBlockPoolMonitorDimensions(new(path), "pool-1"), Instruments);

        monitor.TrackMemoryAllocated(1024);

        listener.Collect();
        Assert.NotEmpty(listener.Measurements);
        Assert.All(listener.Measurements, tags => Assert.Equal("pool-1", tags["ObjectPoolId"]));
    }
}
