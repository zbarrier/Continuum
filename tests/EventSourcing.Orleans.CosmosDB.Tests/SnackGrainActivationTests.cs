using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Snacks;

using Microsoft.Extensions.DependencyInjection;

using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

[Collection(ClusterCollection.Name)]
public class SnackGrainActivationTests(ClusterFixture fixture)
{
    [Fact]
    public async Task Should_Rebuild_State_From_Existing_Log_On_Activation()
    {
        var snackId = Guid.NewGuid();

        // Seed the log before the grain is ever activated, so there is no snapshot and state must come from CosmosDB.
        var siloServices = (fixture.Cluster.Primary as InProcessSiloHandle ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;
        var storage = siloServices.GetRequiredService<ILogConsistentStorage>();
        SnackEvent[] seeded =
        [
            new SnackInitializedEvent(snackId, "Apple", DateTimeOffset.UtcNow),
            new SnackNameChangedEvent(snackId, "Orange", DateTimeOffset.UtcNow),
        ];
        await storage.AppendAsync("snack", GrainId.Create("snack", snackId.ToString()), seeded, 0, Guid.NewGuid());

        var grainFactory = fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId.ToString());

        var getResult = await snackGrain.GetAsync();
        Assert.True(getResult.IsSuccess);
        Assert.Equal("Orange", getResult.Value.Name);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple"));
        Assert.True(initializeResult.IsFailure);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Banana"));
        Assert.True(changeNameResult.IsSuccess);
        Assert.Equal(3, changeNameResult.Value.Version);
    }
}
