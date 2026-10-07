using System.Text.Json;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that <see cref="ProjectionGrain{TGrain, TState}" /> subscribes on activation and projects the
///     events the provider delivers into its persistent state.
/// </summary>
[Collection(ClusterCollection.Name)]
public class StreamProjectionGrainTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";
    private const string EventTypeName = "Tests.ChatMessage";

    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public StreamProjectionGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Projects_Delivered_Events_Into_Persistent_State()
    {
        // The grain derives its stream id from its own key, so the key has to match the stream key the default
        // mapper produces from the stream name.
        var key = Guid.NewGuid().ToString("N");
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key}";

        var projection = _fixture.Cluster.Client.GetGrain<IChatProjectionGrain>(key);

        // Activates the grain, which is what establishes the subscription. Without this the events below would be
        // appended before anything was listening.
        await projection.GetAppliedCount();

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, streamName, new ChatMessage("Ada", "first", DateTimeOffset.UtcNow));
        await AppendAsync(client, streamName, new ChatMessage("Grace", "second", DateTimeOffset.UtcNow));

        var authors = await WaitForAsync(projection, expectedCount: 2);

        Assert.Equal(new[] { "Ada", "Grace" }, authors);
        Assert.Equal(2, await projection.GetAppliedCount());
    }

    [Fact]
    public async Task Does_Not_Reapply_Events_After_The_Grain_Reactivates()
    {
        var key = Guid.NewGuid().ToString("N");
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key}";

        var projection = _fixture.Cluster.Client.GetGrain<IChatProjectionGrain>(key);
        await projection.GetAppliedCount();

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, streamName, new ChatMessage("Ada", "first", DateTimeOffset.UtcNow));
        await WaitForAsync(projection, expectedCount: 1);

        // A fresh subscription may replay from an earlier position. The persisted watermark is what keeps the
        // already applied event from being counted a second time.
        await _fixture.Cluster.Client.GetGrain<IManagementGrain>(0)
            .ForceActivationCollection(TimeSpan.Zero);

        await AppendAsync(client, streamName, new ChatMessage("Grace", "second", DateTimeOffset.UtcNow));

        var authors = await WaitForAsync(projection, expectedCount: 2);

        Assert.Equal(new[] { "Ada", "Grace" }, authors);
        Assert.Equal(2, await projection.GetAppliedCount());
    }

    private static Task AppendAsync(KurrentDBClient client, string streamName, ChatMessage message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data);
        return client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
    }

    private async Task<string[]> WaitForAsync(IChatProjectionGrain projection, int expectedCount)
    {
        // The $all subscription and the Orleans pulling agent both poll, so delivery is not immediate.
        var deadline = DateTime.UtcNow.AddSeconds(60);
        var authors = Array.Empty<string>();
        while (DateTime.UtcNow < deadline)
        {
            authors = await projection.GetAuthors();
            if (authors.Length >= expectedCount)
            {
                break;
            }
            await Task.Delay(250);
        }
        _output.WriteLine($"Projected {authors.Length} event(s).");
        return authors;
    }
}
