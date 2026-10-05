using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Grains;

using Microsoft.Extensions.DependencyInjection;

using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class SnackGrainActivationTests(ClusterFixture fixture)
{
    [Fact]
    public async Task Should_Rebuild_State_From_Existing_Log_On_Activation()
    {
        var snackId = Guid.NewGuid();

        // Seed the log before the grain is ever activated, so there is no snapshot and state must come from KurrentDB.
        var siloServices = (fixture.Cluster.Primary as InProcessSiloHandle ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;
        var storage = siloServices.GetRequiredService<ILogConsistentStorage>();
        SnackEvent[] seeded =
        [
            new SnackInitializedEvent(snackId, "Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Seed", 1),
            new SnackNameChangedEvent(snackId, "Orange", Guid.NewGuid(), DateTimeOffset.UtcNow, "Seed", 2),
        ];
        await storage.AppendAsync("snack", GrainId.Create("snack", snackId.ToString("N")), seeded, 0);

        var grainFactory = fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId);

        var getResult = await snackGrain.GetAsync();
        Assert.True(getResult.IsSuccess);
        Assert.Equal("Orange", getResult.Value.Name);
        Assert.False(await snackGrain.CanInitializeAsync());

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Banana", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Rebuild_State_From_Existing_Log_On_Activation"));
        Assert.True(changeNameResult.IsSuccess);
        Assert.Equal(3, changeNameResult.Value.Version);
    }
}
