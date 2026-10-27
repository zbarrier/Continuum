using System.Numerics;

using Azure.Data.Tables;

using Continuum.Orleans.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Streaming.Orleans.Azure.Storage.Tests;

[Collection(AzuriteCollection.Name)]
public sealed class AzureTableCheckpointStoreTests(AzuriteFixture fixture)
{
    private const string PartitionId = "0";

    // Every test uses its own subscription name, so tests sharing the table never see each other's checkpoints.
    private static string NewSubscription() => $"subscription-{Guid.NewGuid():N}";

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private AzureTableCheckpointStore<TPosition> Store<TPosition>()
        where TPosition : IComparable<TPosition>, ISpanFormattable, IParsable<TPosition>
        => new(fixture.ServiceClient);

    [Fact]
    public async Task Returns_Default_When_No_Checkpoint_Is_Stored()
    {
        var position = await Store<ulong>().GetLastCheckpointAsync(NewSubscription(), PartitionId, Token);

        Assert.Equal(0UL, position);
    }

    [Fact]
    public async Task Returns_Default_BigInteger_When_No_Checkpoint_Is_Stored()
    {
        var position = await Store<BigInteger>().GetLastCheckpointAsync(NewSubscription(), PartitionId, Token);

        Assert.Equal(BigInteger.Zero, position);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(42UL)]
    [InlineData((ulong)long.MaxValue)]
    [InlineData((ulong)long.MaxValue + 1)]
    [InlineData(ulong.MaxValue)]
    public async Task Round_Trips_UInt64_Positions(ulong expected)
    {
        var store = Store<ulong>();
        var subscription = NewSubscription();

        await store.StoreCheckpointAsync(subscription, PartitionId, expected, Token);
        var actual = await store.GetLastCheckpointAsync(subscription, PartitionId, Token);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("18446744073709551615")]
    // A Kinesis sequence number: wider than 64 bits.
    [InlineData("49590338271490256608559692538361571095921575989136588898")]
    public async Task Round_Trips_BigInteger_Positions(string value)
    {
        var expected = BigInteger.Parse(value);
        var store = Store<BigInteger>();
        var subscription = NewSubscription();

        await store.StoreCheckpointAsync(subscription, PartitionId, expected, Token);
        var actual = await store.GetLastCheckpointAsync(subscription, PartitionId, Token);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Overwrites_The_Previous_Checkpoint()
    {
        var store = Store<ulong>();
        var subscription = NewSubscription();

        await store.StoreCheckpointAsync(subscription, PartitionId, 10, Token);
        await store.StoreCheckpointAsync(subscription, PartitionId, 20, Token);

        Assert.Equal(20UL, await store.GetLastCheckpointAsync(subscription, PartitionId, Token));
    }

    [Fact]
    public async Task Does_Not_Move_The_Checkpoint_Backwards()
    {
        var store = Store<ulong>();
        var subscription = NewSubscription();

        await store.StoreCheckpointAsync(subscription, PartitionId, 20, Token);
        await store.StoreCheckpointAsync(subscription, PartitionId, 10, Token);

        Assert.Equal(20UL, await store.GetLastCheckpointAsync(subscription, PartitionId, Token));
    }

    [Fact]
    public async Task Concurrent_Writers_Keep_The_Highest_Checkpoint()
    {
        var subscription = NewSubscription();

        await Task.WhenAll(Enumerable.Range(1, 20).Select(i => Store<ulong>().StoreCheckpointAsync(subscription, PartitionId, (ulong)i, Token)));

        Assert.Equal(20UL, await Store<ulong>().GetLastCheckpointAsync(subscription, PartitionId, Token));
    }

    [Fact]
    public async Task Keeps_Partitions_Of_A_Subscription_Separate()
    {
        var store = Store<ulong>();
        var subscription = NewSubscription();

        await store.StoreCheckpointAsync(subscription, "shard-0", 10, Token);
        await store.StoreCheckpointAsync(subscription, "shard-1", 4, Token);

        Assert.Equal(10UL, await store.GetLastCheckpointAsync(subscription, "shard-0", Token));
        Assert.Equal(4UL, await store.GetLastCheckpointAsync(subscription, "shard-1", Token));
    }

    [Fact]
    public async Task Keeps_Subscriptions_Separate()
    {
        var store = Store<ulong>();
        var first = NewSubscription();
        var second = NewSubscription();

        await store.StoreCheckpointAsync(first, PartitionId, 10, Token);
        await store.StoreCheckpointAsync(second, PartitionId, 4, Token);

        Assert.Equal(10UL, await store.GetLastCheckpointAsync(first, PartitionId, Token));
        Assert.Equal(4UL, await store.GetLastCheckpointAsync(second, PartitionId, Token));
    }

    [Fact]
    public async Task Registers_A_Keyed_Store()
    {
        using var host = BuildHost(builder => builder.AddAzureTableCheckpointStore<ulong>(ConnectionName));

        var store = host.Services.GetRequiredKeyedService<ICheckpointStore<ulong>>(ConnectionName);

        Assert.IsType<AzureTableCheckpointStore<ulong>>(store);
        Assert.Null(host.Services.GetService<ICheckpointStore<ulong>>());
    }

    [Fact]
    public async Task Registers_A_Keyed_And_Default_Store()
    {
        using var host = BuildHost(builder => builder.AddAzureTableCheckpointStoreAsDefault<BigInteger>(ConnectionName));

        var keyed = host.Services.GetRequiredKeyedService<ICheckpointStore<BigInteger>>(ConnectionName);
        var unkeyed = host.Services.GetRequiredService<ICheckpointStore<BigInteger>>();

        Assert.IsType<AzureTableCheckpointStore<BigInteger>>(keyed);
        Assert.Same(keyed, unkeyed);
    }

    private const string ConnectionName = "checkpoints";

    private IHost BuildHost(Action<IHostApplicationBuilder> configure)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddKeyedSingleton(ConnectionName, fixture.ServiceClient);
        configure(builder);
        return builder.Build();
    }
}
