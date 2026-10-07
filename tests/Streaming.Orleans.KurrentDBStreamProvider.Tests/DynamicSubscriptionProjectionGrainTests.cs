using System.Text.Json;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that a <see cref="ProjectionGrain{TGrain, TState}" /> can subscribe to and unsubscribe from individual
///     streams at runtime, and that runtime subscriptions survive reactivation when the grain records them.
/// </summary>
[Collection(ClusterCollection.Name)]
public class DynamicSubscriptionProjectionGrainTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";
    private const string EventTypeName = "Tests.ChatMessage";

    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public DynamicSubscriptionProjectionGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Projects_Only_Streams_Followed_At_Runtime()
    {
        var key = Guid.NewGuid().ToString("N");
        var followed = $"{key}-followed";
        var ignored = $"{key}-ignored";

        var projection = _fixture.Cluster.Client.GetGrain<IDynamicSubscriptionProjectionGrain>(key);
        await projection.Follow(followed);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, ignored, "Grace");
        await AppendAsync(client, followed, "Ada");

        var authors = await WaitForAsync(projection, expectedCount: 1);

        Assert.Equal(new[] { "Ada" }, authors);
    }

    [Fact]
    public async Task Following_The_Same_Stream_Twice_Does_Not_Duplicate_Delivery()
    {
        var key = Guid.NewGuid().ToString("N");
        var followed = $"{key}-followed";

        var projection = _fixture.Cluster.Client.GetGrain<IDynamicSubscriptionProjectionGrain>(key);
        await projection.Follow(followed);
        await projection.Follow(followed);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, followed, "Ada");

        await WaitForAsync(projection, expectedCount: 1);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "Ada" }, await projection.GetAuthors());
    }

    [Fact]
    public async Task Stops_Projecting_A_Stream_After_Unfollowing_It()
    {
        var key = Guid.NewGuid().ToString("N");
        var first = $"{key}-first";
        var second = $"{key}-second";

        var projection = _fixture.Cluster.Client.GetGrain<IDynamicSubscriptionProjectionGrain>(key);
        await projection.Follow(first);
        await projection.Follow(second);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, first, "Ada");
        await WaitForAsync(projection, expectedCount: 1);

        await projection.Unfollow(first);

        // The event on the unfollowed stream is written first, so once the second stream's event has been
        // projected the first one has had the same opportunity to arrive.
        await AppendAsync(client, first, "Grace");
        await AppendAsync(client, second, "Linus");

        await WaitForAsync(projection, expectedCount: 2);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "Ada", "Linus" }, await projection.GetAuthors());
    }

    [Fact]
    public async Task Resumes_Runtime_Subscriptions_After_The_Grain_Reactivates()
    {
        var key = Guid.NewGuid().ToString("N");
        var followed = $"{key}-followed";

        var projection = _fixture.Cluster.Client.GetGrain<IDynamicSubscriptionProjectionGrain>(key);
        await projection.Follow(followed);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, followed, "Ada");
        await WaitForAsync(projection, expectedCount: 1);

        await _fixture.Cluster.Client.GetGrain<IManagementGrain>(0)
            .ForceActivationCollection(TimeSpan.Zero);

        await AppendAsync(client, followed, "Grace");

        var authors = await WaitForAsync(projection, expectedCount: 2);

        Assert.Equal(new[] { "Ada", "Grace" }, authors);
    }

    private static Task AppendAsync(KurrentDBClient client, string streamKey, string author)
    {
        var message = new ChatMessage(author, "hello", DateTimeOffset.UtcNow);
        var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data);
        return client.AppendToStreamAsync(
            $"{Constants.EventSourcedStreamPrefix}-{streamKey}",
            StreamState.Any,
            new[] { eventData });
    }

    private async Task<string[]> WaitForAsync(IDynamicSubscriptionProjectionGrain projection, int expectedCount)
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
            await Task.Delay(250, TestContext.Current.CancellationToken);
        }
        _output.WriteLine($"Projected {authors.Length} event(s): [{string.Join(",", authors)}]");
        return authors;
    }
}
