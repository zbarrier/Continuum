using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Commands;
using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Grains;

using Microsoft.Extensions.DependencyInjection;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

[Collection(ClusterCollection.Name)]
public class SnackRepoGrainTests
{
    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SnackRepoGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Should_Create_A_New_SnackGrain()
    {
        var snackId1 = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackRepoGrain = grainFactory.GetGrain<ISnackCrudRepoGrain>(Guid.Empty);
        Assert.NotNull(snackRepoGrain);

        var createResult1 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId1, "Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_A_New_SnackGrain"));
        Assert.True(createResult1.IsSuccess);
    }

    [Fact]
    public async Task Should_Create_Many_New_SnackGrains()
    {
        var snackId1 = Guid.NewGuid();
        var snackId2 = Guid.NewGuid();
        var snackId3 = Guid.NewGuid();
        var snackId4 = Guid.NewGuid();
        var snackId5 = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackRepoGrain = grainFactory.GetGrain<ISnackCrudRepoGrain>(Guid.Empty);

        var createResult0 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId1, "Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_Many_New_SnackGrains"));
        var createResult1 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId1, "Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_Many_New_SnackGrains"));
        var createResult2 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId2, "Orange", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_Many_New_SnackGrains"));
        var createResult3 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId3, "Cafe", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_Many_New_SnackGrains"));
        var createResult4 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId4, "Coke", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_Many_New_SnackGrains"));
        var createResult5 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId5, "Banana", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_Many_New_SnackGrains"));
        
        Assert.True(createResult0.IsSuccess);
        Assert.True(createResult1.IsFailure);
        Assert.True(createResult2.IsSuccess);
        Assert.True(createResult3.IsSuccess);
        Assert.True(createResult4.IsSuccess);
        Assert.True(createResult5.IsSuccess);
    }

    [Fact]
    public async Task Should_Get_SnackGrain()
    {
        var snackId1 = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackRepoGrain = grainFactory.GetGrain<ISnackCrudRepoGrain>(Guid.Empty);
        Assert.NotNull(snackRepoGrain);

        var createResult1 = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId1, "Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Get_SnackGrain"));
        Assert.True(createResult1.IsSuccess);

        var getResult1 = await snackRepoGrain.GetAsync(new SnackRepoGetCommand(snackId1, Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Get_SnackGrain"));
        Assert.True(getResult1.IsSuccess);
        Assert.NotNull(getResult1.Value);

        var snackState = await getResult1.Value.GetAsync();
        Assert.Equal("Apple", snackState.Value.Name);
    }

    [Fact]
    public async Task Should_Get_Many_SnackGrains()
    {
        var snackId1 = Guid.NewGuid();
        var snackId2 = Guid.NewGuid();
        var snackId3 = Guid.NewGuid();
        var snackId4 = Guid.NewGuid();
        var snackId5 = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackRepoGrain = grainFactory.GetGrain<ISnackCrudRepoGrain>(Guid.Empty);
        Assert.NotNull(snackRepoGrain);

        var snackIds = new[] { snackId1, snackId2, snackId3, snackId4, snackId5 };
        foreach (var snackId in snackIds)
        {
            var createResult = await snackRepoGrain.CreateAsync(new SnackRepoCreateCommand(snackId, "Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Get_Many_SnackGrains"));
            Assert.True(createResult.IsSuccess);
        }

        var getManyResult = await snackRepoGrain.GetManyAsync(
            new SnackRepoGetManyCommand(snackIds,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                "Should_Get_Many_SnackGrains"));
        Assert.True(getManyResult.IsSuccess);
        Assert.Equal(5, getManyResult.Value.Count);

        foreach (var grain in getManyResult.Value)
        {
            var snack = await grain.GetAsync();
            Assert.True(snack.IsSuccess);
            _output.WriteLine(snack.Value.ToString());
        }
    }
}
