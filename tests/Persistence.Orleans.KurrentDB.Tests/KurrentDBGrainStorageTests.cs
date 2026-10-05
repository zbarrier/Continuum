using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class KurrentDBGrainStorageTests(ClusterFixture fixture)
{
    private const string GrainTypeName = "counter";

    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private IGrainStorage MarkerStorage => SiloServices.GetRequiredKeyedService<IGrainStorage>(TestSiloConfigurations.MarkerProvider);

    private IGrainStorage DeleteStorage => SiloServices.GetRequiredKeyedService<IGrainStorage>(TestSiloConfigurations.DeleteProvider);

    private KurrentDBClient Client => SiloServices.GetRequiredKeyedService<KurrentDBClient>(TestSiloConfigurations.ConnectionName);

    private static GrainId NewGrainId() => GrainId.Create(GrainTypeName, Guid.NewGuid().ToString("N"));

    private static string StreamName(GrainId grainId) => $"state__{grainId.Type}-{grainId.Key}";

    private static GrainState<CounterState> NewState(int count = 0, string? label = null) =>
        new(new CounterState { Count = count, Label = label });

    [Fact]
    public async Task Should_Read_Missing_State_As_Not_Existing()
    {
        var state = NewState();

        await MarkerStorage.ReadStateAsync(GrainTypeName, NewGrainId(), state);

        Assert.False(state.RecordExists);
        Assert.Null(state.ETag);
        Assert.Equal(0, state.State!.Count);
    }

    [Fact]
    public async Task Should_Write_And_Read_State()
    {
        var grainId = NewGrainId();
        var written = NewState(5, "five");

        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, written);

        Assert.True(written.RecordExists);
        Assert.Equal("0", written.ETag);

        var read = NewState();
        await MarkerStorage.ReadStateAsync(GrainTypeName, grainId, read);

        Assert.True(read.RecordExists);
        Assert.Equal("0", read.ETag);
        Assert.Equal(5, read.State!.Count);
        Assert.Equal("five", read.State!.Label);
    }

    [Fact]
    public async Task Should_Advance_ETag_On_Each_Write()
    {
        var grainId = NewGrainId();
        var state = NewState(1);

        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, state);
        state.State!.Count = 2;
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, state);

        Assert.Equal("1", state.ETag);

        var read = NewState();
        await MarkerStorage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.Equal(2, read.State!.Count);
        Assert.Equal("1", read.ETag);
    }

    [Fact]
    public async Task Should_Store_State_With_Mapped_Type_And_Json_Content()
    {
        var grainId = NewGrainId();
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, NewState(3));

        var resolved = await Client.ReadStreamAsync(Direction.Forwards, StreamName(grainId), StreamPosition.Start, cancellationToken: TestContext.Current.CancellationToken)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal("persistence-test-counter-state", resolved.Event.EventType);
        Assert.Equal("application/json", resolved.Event.ContentType);
    }

    [Fact]
    public async Task Should_Throw_InconsistentState_When_Writing_With_Stale_ETag()
    {
        var grainId = NewGrainId();
        var first = NewState(1);
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, first);
        var stale = NewState(1);
        stale.ETag = first.ETag;
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, first);

        var ex = await Assert.ThrowsAsync<InconsistentStateException>(() => MarkerStorage.WriteStateAsync(GrainTypeName, grainId, stale));

        Assert.Equal("1", ex.StoredEtag);
        Assert.Equal("0", ex.CurrentEtag);
        Assert.IsType<WrongExpectedVersionException>(ex.InnerException);
    }

    [Fact]
    public async Task Should_Throw_InconsistentState_When_Writing_Without_ETag_To_Existing_Stream()
    {
        var grainId = NewGrainId();
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, NewState(1));

        var ex = await Assert.ThrowsAsync<InconsistentStateException>(() => MarkerStorage.WriteStateAsync(GrainTypeName, grainId, NewState(2)));

        Assert.Equal("0", ex.StoredEtag);
        Assert.Null(ex.CurrentEtag);
    }

    [Fact]
    public async Task Should_Clear_State_With_Marker_And_Keep_Stream()
    {
        var grainId = NewGrainId();
        var state = NewState(4);
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, state);

        await MarkerStorage.ClearStateAsync(GrainTypeName, grainId, state);

        Assert.False(state.RecordExists);
        Assert.Equal("1", state.ETag);

        var read = NewState();
        await MarkerStorage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.False(read.RecordExists);
        Assert.Equal("1", read.ETag);

        // The kept ETag lets the grain write again after clearing.
        state.State!.Count = 7;
        await MarkerStorage.WriteStateAsync(GrainTypeName, grainId, state);
        await MarkerStorage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.True(read.RecordExists);
        Assert.Equal(7, read.State!.Count);
    }

    [Fact]
    public async Task Should_Clear_State_By_Deleting_Stream()
    {
        var grainId = NewGrainId();
        var state = NewState(4);
        await DeleteStorage.WriteStateAsync(GrainTypeName, grainId, state);

        await DeleteStorage.ClearStateAsync(GrainTypeName, grainId, state);

        Assert.False(state.RecordExists);
        Assert.Null(state.ETag);

        var read = NewState();
        await DeleteStorage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.False(read.RecordExists);
        Assert.Null(read.ETag);

        // A deleted stream accepts a fresh write and reads back straight away. KurrentDB keeps revisions
        // increasing after a soft delete.
        state.State!.Count = 9;
        await DeleteStorage.WriteStateAsync(GrainTypeName, grainId, state);
        Assert.True(state.RecordExists);
        Assert.Equal("1", state.ETag);

        await DeleteStorage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.True(read.RecordExists);
        Assert.Equal("1", read.ETag);
        Assert.Equal(9, read.State!.Count);
    }

    [Theory]
    [InlineData(TestSiloConfigurations.MarkerProvider)]
    [InlineData(TestSiloConfigurations.DeleteProvider)]
    public async Task Should_Do_Nothing_When_Clearing_Missing_State(string provider)
    {
        var storage = SiloServices.GetRequiredKeyedService<IGrainStorage>(provider);
        var state = NewState();

        await storage.ClearStateAsync(GrainTypeName, NewGrainId(), state);

        Assert.False(state.RecordExists);
        Assert.Null(state.ETag);
    }

    [Theory]
    [InlineData(TestSiloConfigurations.MarkerProvider)]
    [InlineData(TestSiloConfigurations.DeleteProvider)]
    public async Task Should_Throw_InconsistentState_When_Clearing_Without_ETag_And_Stream_Exists(string provider)
    {
        var storage = SiloServices.GetRequiredKeyedService<IGrainStorage>(provider);
        var grainId = NewGrainId();
        await storage.WriteStateAsync(GrainTypeName, grainId, NewState(1));

        var ex = await Assert.ThrowsAsync<InconsistentStateException>(() => storage.ClearStateAsync(GrainTypeName, grainId, NewState()));

        Assert.Equal("0", ex.StoredEtag);
        Assert.Null(ex.CurrentEtag);
    }

    [Theory]
    [InlineData(TestSiloConfigurations.MarkerProvider)]
    [InlineData(TestSiloConfigurations.DeleteProvider)]
    public async Task Should_Throw_InconsistentState_When_Clearing_With_Stale_ETag(string provider)
    {
        var storage = SiloServices.GetRequiredKeyedService<IGrainStorage>(provider);
        var grainId = NewGrainId();
        var current = NewState(1);
        await storage.WriteStateAsync(GrainTypeName, grainId, current);
        var stale = NewState(1);
        stale.ETag = current.ETag;
        await storage.WriteStateAsync(GrainTypeName, grainId, current);

        var ex = await Assert.ThrowsAsync<InconsistentStateException>(() => storage.ClearStateAsync(GrainTypeName, grainId, stale));

        Assert.Equal("1", ex.StoredEtag);
        Assert.Equal("0", ex.CurrentEtag);
        Assert.IsType<WrongExpectedVersionException>(ex.InnerException);
    }
}
