using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;

using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.Streams;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Drives the change feed processor callbacks directly to verify that only events handed to the handler are counted
/// and that the subscription stays retrying while any lease is still failing.
/// </summary>
public class CosmosDBCatchupSubscriptionCallbackTests
{
    private const string ConnectionName = "cosmosdb";
    private const string EventType = "test-event";

    [Fact]
    public async Task Counts_only_the_events_handed_to_the_handler()
    {
        using var harness = new Harness();
        long eventsProcessed = 0;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == CosmosDBCatchupSubscriptionMetrics.MeterName && instrument.Name == "continuum.subscription.events.processed")
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref eventsProcessed, value));
        listener.Start();

        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("0"), Changes(
            Event(1, "event-0"),
            Item("hdr", 1, EventType, "\"header\""),
            Item("evt", 2, "unknown-type", "\"unknown\""),
            Item("hdr", 3, EventType, "\"deleted\"", deleted: true),
            Event(4, "event-1")), CancellationToken.None);

        Assert.Equal(["event-0", "event-1"], harness.Subscription.Received);
        Assert.Equal(2, Interlocked.Read(ref eventsProcessed));
    }

    [Fact]
    public async Task Stays_retrying_while_another_lease_is_still_failing()
    {
        using var harness = new Harness();

        await harness.Subscription.HandleErrorAsync("0", UserException("0"));
        await harness.Subscription.HandleErrorAsync("1", UserException("1"));
        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("0"), Changes(Event(1, "event-0")), CancellationToken.None);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Retrying, harness.State);

        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("1"), Changes(Event(1, "event-1")), CancellationToken.None);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Running, harness.State);
    }

    [Fact]
    public async Task A_lease_that_never_failed_does_not_clear_another_lease_retry()
    {
        using var harness = new Harness();

        await harness.Subscription.HandleErrorAsync("0", UserException("0"));
        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("1"), Changes(Event(1, "event-0")), CancellationToken.None);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Retrying, harness.State);
    }

    [Fact]
    public async Task An_infrastructure_error_reports_retrying_until_a_batch_succeeds()
    {
        using var harness = new Harness();
        var error = new InvalidOperationException("The lease was lost.");

        await harness.Subscription.HandleErrorAsync("0", error);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Retrying, harness.State);
        Assert.Same(error, harness.Exception);

        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("1"), Changes(Event(1, "event-0")), CancellationToken.None);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Running, harness.State);
    }

    [Fact]
    public async Task An_infrastructure_error_recovery_does_not_clear_a_failing_lease()
    {
        using var harness = new Harness();

        await harness.Subscription.HandleErrorAsync("0", UserException("0"));
        await harness.Subscription.HandleErrorAsync("1", new InvalidOperationException("The lease was lost."));
        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("2"), Changes(Event(1, "event-0")), CancellationToken.None);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Retrying, harness.State);
    }

    [Fact]
    public async Task Maps_item_metadata_onto_the_streamed_event()
    {
        using var harness = new Harness();
        var withMetadata = Event(1, "event-0").TrimEnd('}') + ""","meta":{"tenant":"t1"}}""";

        await harness.Subscription.HandleChangeStreamAsync(new LeaseContext("0"), Changes(withMetadata, Event(2, "event-1")), CancellationToken.None);

        Assert.Equal("t1", harness.Subscription.ReceivedEvents[0].Metadata["tenant"]);
        Assert.Empty(harness.Subscription.ReceivedEvents[1].Metadata);
    }

    private static ChangeFeedProcessorUserException UserException(string leaseToken)
        => new(new InvalidOperationException("The handler rejected the batch."), new LeaseContext(leaseToken));

    private static string Event(int version, string data) => Item("evt", version, EventType, $"\"{data}\"");

    private static string Item(string type, int version, string dataType, string data, bool deleted = false)
        => $$"""{"id":"{{Guid.NewGuid()}}","type":"{{type}}","streamName":"test/stream","ver":{{version}},"dataType":"{{dataType}}","data":{{data}},"del":{{(deleted ? "true" : "false")}},"subSeq":0,"_ts":0,"_lsn":{{version}}}""";

    private static MemoryStream Changes(params string[] items)
        => new(Encoding.UTF8.GetBytes($$"""{"Documents":[{{string.Join(",", items)}}]}"""));

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _services;

        public Harness()
        {
            var typeMapper = new TestTypeMapper();
            typeMapper.AddType(typeof(string), EventType);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<CosmosDBCatchupSubscriptionStatusRegistry>();
            services.AddSingleton(sp => new CosmosDBCatchupSubscriptionMetrics(sp.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>()));
            services.AddKeyedSingleton(ConnectionName, (_, _) => new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="));
            services.AddOptions<CosmosDBCatchupSubscriptionOptions>(LeaseCatchupSubscription.Name).Configure(options =>
            {
                options.ConnectionName = ConnectionName;
                options.DatabaseName = "database";
                options.MonitoredContainerName = "events";
                options.LeaseContainerName = "leases";
                options.InstanceName = "test-instance";
                options.GrainStorageSerializer = new JsonStringSerializer();
                options.TypeMapper = typeMapper;
                options.StreamNameParser = new DefaultCosmosDBStreamNameParser();
            });
            _services = services.BuildServiceProvider();
            Subscription = new LeaseCatchupSubscription(_services);
        }

        public LeaseCatchupSubscription Subscription { get; }

        public CosmosDBCatchupSubscriptionState? State
            => _services.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>().TryGetStatus(LeaseCatchupSubscription.Name, out var status) ? status!.State : null;

        public Exception? Exception
            => _services.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>().TryGetStatus(LeaseCatchupSubscription.Name, out var status) ? status!.Exception : null;

        public void Dispose() => _services.Dispose();
    }

    private sealed class LeaseCatchupSubscription(IServiceProvider serviceProvider)
        : CosmosDBCatchupSubscription<LeaseCatchupSubscription, CosmosDBCatchupSubscriptionOptions>(serviceProvider)
    {
        public List<string> Received { get; } = [];

        public List<IStreamedEvent<object>> ReceivedEvents { get; } = [];

        protected override Task HandleStreamEventsAsync(ChangeFeedProcessorContext context, List<IStreamedEvent<object>> streamEvents, CancellationToken cancellationToken)
        {
            ReceivedEvents.AddRange(streamEvents);
            Received.AddRange(streamEvents.Select(streamEvent => (string)streamEvent.Event));
            return Task.CompletedTask;
        }
    }

    private sealed class LeaseContext(string leaseToken) : ChangeFeedProcessorContext
    {
        public override string LeaseToken => leaseToken;
        public override CosmosDiagnostics Diagnostics => null!;
        public override Headers Headers { get; } = new();
        public override FeedRange FeedRange => null!;
    }

    private sealed class TestTypeMapper : TypeMapper;

    private sealed class JsonStringSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => (T)(object)JsonSerializer.Deserialize<string>(input.ToString())!;
    }
}
