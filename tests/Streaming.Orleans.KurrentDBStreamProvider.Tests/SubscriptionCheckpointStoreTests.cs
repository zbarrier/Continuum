using System.Numerics;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
/// Verifies <see cref="SubscriptionCheckpointStore{TPosition}" /> and <see cref="SubscriptionCheckpointGrain{TPosition}" />.
/// </summary>
[Collection(ClusterCollection.Name)]
public class SubscriptionCheckpointStoreTests
{
    private readonly ClusterFixture _fixture;

    public SubscriptionCheckpointStoreTests(ClusterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Returns_Default_For_Unknown_Subscription()
    {
        var store = new SubscriptionCheckpointStore<ulong>(_fixture.Cluster.GrainFactory);

        Assert.Equal(0UL, await store.GetLastCheckpointAsync(Guid.NewGuid().ToString("N"), "0", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Stores_Checkpoint_And_Ignores_Older_Positions()
    {
        var store = new SubscriptionCheckpointStore<ulong>(_fixture.Cluster.GrainFactory);
        var subscription = Guid.NewGuid().ToString("N");

        await store.StoreCheckpointAsync(subscription, "0", 10, TestContext.Current.CancellationToken);
        await store.StoreCheckpointAsync(subscription, "0", 5, TestContext.Current.CancellationToken);

        Assert.Equal(10UL, await store.GetLastCheckpointAsync(subscription, "0", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Separates_Checkpoints_By_Partition()
    {
        var store = new SubscriptionCheckpointStore<ulong>(_fixture.Cluster.GrainFactory);
        var subscription = Guid.NewGuid().ToString("N");

        await store.StoreCheckpointAsync(subscription, "0", 7, TestContext.Current.CancellationToken);
        await store.StoreCheckpointAsync(subscription, "1", 3, TestContext.Current.CancellationToken);

        Assert.Equal(7UL, await store.GetLastCheckpointAsync(subscription, "0", TestContext.Current.CancellationToken));
        Assert.Equal(3UL, await store.GetLastCheckpointAsync(subscription, "1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Stores_BigInteger_Checkpoint()
    {
        var store = new SubscriptionCheckpointStore<BigInteger>(_fixture.Cluster.GrainFactory);
        var subscription = Guid.NewGuid().ToString("N");
        var position = BigInteger.Parse("123456789012345678901234567890");

        await store.StoreCheckpointAsync(subscription, "0", position, TestContext.Current.CancellationToken);
        await store.StoreCheckpointAsync(subscription, "0", 5, TestContext.Current.CancellationToken);

        Assert.Equal(position, await store.GetLastCheckpointAsync(subscription, "0", TestContext.Current.CancellationToken));
    }
}
