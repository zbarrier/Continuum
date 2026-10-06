using System.Text.Json;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that a <see cref="StreamProjectionGrain{TGrain, TState}" /> works under an implicit subscription,
///     where the runtime activates the grain because an event arrived rather than because a caller asked for it.
/// </summary>
/// <remarks>
///     This is the behaviour the explicit tests cannot cover. There the test activates the grain first and the
///     subscription is established before anything is appended; here nothing touches the grain until the assertions
///     run, so the projection only sees the events if the runtime activated it on their arrival.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class ImplicitStreamProjectionGrainTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";
    private const string EventTypeName = "Tests.ChatMessage";

    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ImplicitStreamProjectionGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Activates_The_Grain_And_Projects_Without_The_Grain_Being_Called_First()
    {
        var key = Guid.NewGuid().ToString("N");
        var streamName = $"{Constants.ImplicitStreamPrefix}-{key}";

        // Deliberately no call on the grain before appending. Under an implicit subscription the arrival of the
        // event is what activates it.
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, streamName, new ChatMessage("Ada", "first", DateTimeOffset.UtcNow));
        await AppendAsync(client, streamName, new ChatMessage("Grace", "second", DateTimeOffset.UtcNow));

        var projection = _fixture.Cluster.Client.GetGrain<IImplicitChatProjectionGrain>(key);
        var authors = await WaitForAsync(projection, expectedCount: 2);

        Assert.Equal(new[] { "Ada", "Grace" }, authors);
    }

    [Fact]
    public async Task Routes_Each_Stream_To_The_Grain_Whose_Key_Matches()
    {
        var firstKey = Guid.NewGuid().ToString("N");
        var secondKey = Guid.NewGuid().ToString("N");

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, $"{Constants.ImplicitStreamPrefix}-{firstKey}", new ChatMessage("Ada", "first", DateTimeOffset.UtcNow));
        await AppendAsync(client, $"{Constants.ImplicitStreamPrefix}-{secondKey}", new ChatMessage("Grace", "second", DateTimeOffset.UtcNow));

        var first = _fixture.Cluster.Client.GetGrain<IImplicitChatProjectionGrain>(firstKey);
        var second = _fixture.Cluster.Client.GetGrain<IImplicitChatProjectionGrain>(secondKey);

        var firstAuthors = await WaitForAsync(first, expectedCount: 1);
        var secondAuthors = await WaitForAsync(second, expectedCount: 1);

        _output.WriteLine($"first key {firstKey} saw [{string.Join(",", firstAuthors)}]");
        _output.WriteLine($"second key {secondKey} saw [{string.Join(",", secondAuthors)}]");

        // Each grain must see only its own stream. A single shared subscription, or a stream id that did not derive
        // from the grain key, would show both events on both grains.
        Assert.Equal(new[] { "Ada" }, firstAuthors);
        Assert.Equal(new[] { "Grace" }, secondAuthors);
    }

    private static Task AppendAsync(KurrentDBClient client, string streamName, ChatMessage message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data);
        return client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
    }

    private async Task<string[]> WaitForAsync(IImplicitChatProjectionGrain projection, int expectedCount)
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
        _output.WriteLine($"Projected {authors.Length} event(s).");
        return authors;
    }
}
