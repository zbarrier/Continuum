using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Tallies;

using Microsoft.Extensions.DependencyInjection;

using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream;

/// <summary>
///     Tests for a grain with <see cref="Continuum.Orleans.Events.LoadMostRecentEventsOnlyAttribute"/>, which loads only
///     its <see cref="TallyGrain.MostRecentEventCount"/> (3) most recent events.
/// </summary>
[Collection(ClusterCollection.Name)]
public class TallyGrainActivationTests(ClusterFixture fixture)
{
    private ILogConsistentStorage Storage => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services.GetRequiredService<ILogConsistentStorage>();

    private ITallyGrain Tally(string id) => fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>().GetGrain<ITallyGrain>(id);

    private Task AppendAsync(string id, int expectedVersion, params int[] values)
        => Storage.AppendAsync(TallyGrain.GrainTypeName, GrainId.Create(TallyGrain.GrainTypeName, id),
            values.Select(value => new TallyAdded(value)).ToArray(), expectedVersion, Guid.NewGuid());

    private async Task AddThroughTheGrainAndDeactivateAsync(string id, params int[] values)
    {
        foreach (var value in values)
        {
            await Tally(id).AddAsync(value);
        }
        await Tally(id).DeactivateAsync();
    }

    [Fact]
    public async Task Should_Load_Only_The_Most_Recent_Events_When_There_Is_No_Snapshot()
    {
        var id = Guid.NewGuid().ToString("N");
        // Seeding the log directly leaves the grain without a snapshot.
        await AppendAsync(id, 0, 1, 2, 3, 4, 5);

        var loaded = await Tally(id).GetAsync();

        Assert.Equal(5, loaded.Version);
        Assert.Equal([3, 4, 5], loaded.Values);
    }

    [Fact]
    public async Task Should_Append_At_The_Logs_Real_Version()
    {
        var id = Guid.NewGuid().ToString("N");
        await AppendAsync(id, 0, 1, 2, 3, 4, 5);

        Assert.Equal(6, await Tally(id).AddAsync(6));

        var loaded = await Tally(id).GetAsync();
        Assert.Equal([3, 4, 5, 6], loaded.Values);
        var (version, entries) = await Storage.ReadMostRecentAsync<TallyAdded>(TallyGrain.GrainTypeName, GrainId.Create(TallyGrain.GrainTypeName, id), 10);
        Assert.Equal(6, version);
        Assert.Equal([1, 2, 3, 4, 5, 6], entries.Select(entry => entry.Value));
    }

    [Fact]
    public async Task Should_Load_The_Whole_Log_When_It_Has_Fewer_Events_Than_The_Count()
    {
        var id = Guid.NewGuid().ToString("N");
        await AppendAsync(id, 0, 1, 2);

        var loaded = await Tally(id).GetAsync();

        Assert.Equal(2, loaded.Version);
        Assert.Equal([1, 2], loaded.Values);
    }

    [Fact]
    public async Task Should_Apply_The_Events_After_A_Snapshot_Within_The_Most_Recent_Events()
    {
        var id = Guid.NewGuid().ToString("N");
        // Every write through the grain saves a snapshot, here holding all 5 values at version 5.
        await AddThroughTheGrainAndDeactivateAsync(id, 1, 2, 3, 4, 5);
        await AppendAsync(id, 5, 6, 7);

        var loaded = await Tally(id).GetAsync();

        // The snapshot is only 2 events behind, so it is kept, with its older values, and the 2 events are applied.
        Assert.Equal(7, loaded.Version);
        Assert.Equal([1, 2, 3, 4, 5, 6, 7], loaded.Values);
    }

    [Fact]
    public async Task Should_Start_From_A_New_State_When_The_Snapshot_Is_Further_Behind_Than_The_Most_Recent_Events()
    {
        var id = Guid.NewGuid().ToString("N");
        await AddThroughTheGrainAndDeactivateAsync(id, 1, 2, 3, 4, 5);
        await AppendAsync(id, 5, 6, 7, 8, 9);

        var loaded = await Tally(id).GetAsync();

        // The snapshot is 4 events behind, so event 6 is never read and the snapshot cannot be caught up.
        Assert.Equal(9, loaded.Version);
        Assert.Equal([7, 8, 9], loaded.Values);
    }
}
