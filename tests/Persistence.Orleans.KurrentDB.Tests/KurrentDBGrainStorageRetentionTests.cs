using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class KurrentDBGrainStorageRetentionTests(ClusterFixture fixture)
{
    private const string GrainTypeName = "counter";
    private const int DefaultMaxCount = 5;

    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private IGrainStorage Storage(string provider) => SiloServices.GetRequiredKeyedService<IGrainStorage>(provider);

    private KurrentDBClient Client => SiloServices.GetRequiredKeyedService<KurrentDBClient>(TestSiloConfigurations.ConnectionName);

    private static GrainId NewGrainId() => GrainId.Create(GrainTypeName, Guid.NewGuid().ToString("N"));

    private static string StreamName(GrainId grainId) => $"state__{grainId.Type}-{grainId.Key}";

    private async Task<int?> GetMaxCountAsync(GrainId grainId) =>
        (await Client.GetStreamMetadataAsync(StreamName(grainId), cancellationToken: TestContext.Current.CancellationToken)).Metadata.MaxCount;

    private async Task<List<long>> ReadRevisionsAsync(GrainId grainId) =>
        await Client.ReadStreamAsync(Direction.Forwards, StreamName(grainId), StreamPosition.Start, cancellationToken: TestContext.Current.CancellationToken)
            .Select(e => e.Event.EventNumber.ToInt64())
            .ToListAsync(TestContext.Current.CancellationToken);

    private static async Task WriteManyAsync(IGrainStorage storage, GrainId grainId, GrainState<CounterState> state, int count)
    {
        for (var index = 0; index < count; index++)
        {
            state.State!.Count = index;
            await storage.WriteStateAsync(GrainTypeName, grainId, state);
        }
    }

    [Fact]
    public async Task Should_Set_Max_Count_When_Creating_The_Stream()
    {
        var grainId = NewGrainId();

        await Storage(TestSiloConfigurations.MarkerProvider).WriteStateAsync(GrainTypeName, grainId, new GrainState<CounterState>(new CounterState()));

        Assert.Equal(DefaultMaxCount, await GetMaxCountAsync(grainId));
    }

    [Fact]
    public async Task Should_Keep_Only_The_Newest_Events()
    {
        var storage = Storage(TestSiloConfigurations.MarkerProvider);
        var grainId = NewGrainId();
        var state = new GrainState<CounterState>(new CounterState());

        await WriteManyAsync(storage, grainId, state, DefaultMaxCount + 2);

        Assert.Equal([2, 3, 4, 5, 6], await ReadRevisionsAsync(grainId));

        var read = new GrainState<CounterState>(new CounterState());
        await storage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.Equal(DefaultMaxCount + 1, read.State!.Count);
        Assert.Equal("6", read.ETag);
    }

    [Fact]
    public async Task Should_Keep_Max_Count_After_Deleting_And_Recreating_The_Stream()
    {
        var storage = Storage(TestSiloConfigurations.DeleteProvider);
        var grainId = NewGrainId();
        var state = new GrainState<CounterState>(new CounterState());

        await WriteManyAsync(storage, grainId, state, 2);
        await storage.ClearStateAsync(GrainTypeName, grainId, state);
        await WriteManyAsync(storage, grainId, state, DefaultMaxCount + 1);

        Assert.Equal(DefaultMaxCount, await GetMaxCountAsync(grainId));
        Assert.Equal([3, 4, 5, 6, 7], await ReadRevisionsAsync(grainId));

        var read = new GrainState<CounterState>(new CounterState());
        await storage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.Equal(DefaultMaxCount, read.State!.Count);
        Assert.Equal("7", read.ETag);
    }

    [Fact]
    public async Task Should_Not_Limit_The_Stream_When_Max_Count_Is_Null()
    {
        var grainId = NewGrainId();
        var state = new GrainState<CounterState>(new CounterState());

        await WriteManyAsync(Storage(TestSiloConfigurations.UnlimitedProvider), grainId, state, DefaultMaxCount + 2);

        Assert.Null(await GetMaxCountAsync(grainId));
        Assert.Equal(DefaultMaxCount + 2, (await ReadRevisionsAsync(grainId)).Count);
    }
}
