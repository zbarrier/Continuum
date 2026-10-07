using System.Text.Json;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that one <see cref="ProjectionGrain{TGrain, TState}" /> can consume several streams at once.
/// </summary>
/// <remarks>
///     The base class previously held a single subscription handle, so a second subscription overwrote the first.
///     These tests fail against that shape: the events from one of the two streams never reach the projection.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class MultiStreamProjectionGrainTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";
    private const string EventTypeName = "Tests.ChatMessage";

    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MultiStreamProjectionGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Projects_Events_From_Every_Subscribed_Stream()
    {
        var key = Guid.NewGuid().ToString("N");

        // Activate first so both subscriptions exist before anything is appended.
        var projection = _fixture.Cluster.Client.GetGrain<IMultiStreamProjectionGrain>(key);
        await projection.GetAuthors();

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, StreamNameFor(MultiStreamProjectionGrain.FirstStreamKey(key)), new ChatMessage("Ada", "first", DateTimeOffset.UtcNow));
        await AppendAsync(client, StreamNameFor(MultiStreamProjectionGrain.SecondStreamKey(key)), new ChatMessage("Grace", "second", DateTimeOffset.UtcNow));

        var authors = await WaitForAsync(projection, expectedCount: 2);

        // Order between the two streams is not guaranteed, so assert on membership rather than sequence.
        Assert.Equal(new[] { "Ada", "Grace" }, authors.OrderBy(author => author).ToArray());
    }

    [Fact]
    public async Task Keeps_Both_Subscriptions_After_The_Grain_Reactivates()
    {
        var key = Guid.NewGuid().ToString("N");

        var projection = _fixture.Cluster.Client.GetGrain<IMultiStreamProjectionGrain>(key);
        await projection.GetAuthors();

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, StreamNameFor(MultiStreamProjectionGrain.FirstStreamKey(key)), new ChatMessage("Ada", "first", DateTimeOffset.UtcNow));
        await WaitForAsync(projection, expectedCount: 1);

        // Deactivation drops the activation but not the subscriptions, which are durable. Resuming has to restore
        // every stream: restoring only one would leave the grain silently deaf to the other.
        await _fixture.Cluster.Client.GetGrain<IManagementGrain>(0)
            .ForceActivationCollection(TimeSpan.Zero);

        await AppendAsync(client, StreamNameFor(MultiStreamProjectionGrain.SecondStreamKey(key)), new ChatMessage("Grace", "second", DateTimeOffset.UtcNow));

        var authors = await WaitForAsync(projection, expectedCount: 2);

        Assert.Equal(new[] { "Ada", "Grace" }, authors.OrderBy(author => author).ToArray());
    }

    private static string StreamNameFor(string streamKey) => $"{Constants.EventSourcedStreamPrefix}-{streamKey}";

    private static Task AppendAsync(KurrentDBClient client, string streamName, ChatMessage message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data);
        return client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
    }

    private async Task<string[]> WaitForAsync(IMultiStreamProjectionGrain projection, int expectedCount)
    {
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
        _output.WriteLine($"Projected {authors.Length} event(s): [{string.Join(",", authors)}]");
        return authors;
    }
}
