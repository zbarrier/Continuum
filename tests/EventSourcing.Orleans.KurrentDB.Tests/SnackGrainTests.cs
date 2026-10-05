using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Grains;

using Microsoft.Extensions.DependencyInjection;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class SnackGrainTests
{
    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SnackGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Should_Create_A_New_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId);
        Assert.NotNull(snackGrain);       

        var canInitialize = await snackGrain.CanInitializeAsync();  
        Assert.True(canInitialize);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_A_New_Snack"));
        Assert.True(initializeResult.IsSuccess);
        Assert.Equal(1, initializeResult.Value.Version);
        Assert.IsType<SnackInitializedEvent>(Assert.Single(initializeResult.Value.Changes).Event);
    }

    [Fact]
    public async Task Should_Change_Name_Of_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId);
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_A_New_Snack"));
        Assert.True(initializeResult.IsSuccess);

        var canChangeName = await snackGrain.CanChangeNameAsync();
        Assert.True(canChangeName);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Orange", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Change_Name_Of_Snack"));
        Assert.True(changeNameResult.IsSuccess);
        Assert.Equal(2, changeNameResult.Value.Version);
        Assert.IsType<SnackNameChangedEvent>(Assert.Single(changeNameResult.Value.Changes).Event);
    }

    [Fact]
    public async Task Should_Remove_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId);
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_A_New_Snack"));
        Assert.True(initializeResult.IsSuccess);

        var canRemove = await snackGrain.CanRemoveAsync();
        Assert.True(canRemove);

        var removeResult = await snackGrain.RemoveAsync(new SnackRemoveCommand(Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Remove_Snack"));
        Assert.True(removeResult.IsSuccess);
        Assert.Equal(2, removeResult.Value.Version);
        Assert.IsType<SnackRemovedEvent>(Assert.Single(removeResult.Value.Changes).Event);
        Assert.False(await snackGrain.CanRemoveAsync());
    }

    [Fact]
    public async Task Should_Get_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId);
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_A_New_Snack"));
        Assert.True(initializeResult.IsSuccess);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Orange", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Change_Name_Of_Snack"));
        Assert.True(changeNameResult.IsSuccess);

        var getResult = await snackGrain.GetAsync();
        Assert.True(getResult.IsSuccess);

        Assert.Equal("Orange", getResult.Value.Name);
        _output.WriteLine(getResult.Value.ToString());
    }

    [Fact]
    public async Task Should_Get_Events_Of_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId);
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Create_A_New_Snack"));
        Assert.True(initializeResult.IsSuccess);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Orange", Guid.NewGuid(), DateTimeOffset.UtcNow, "Should_Change_Name_Of_Snack"));
        Assert.True(changeNameResult.IsSuccess);

        // Versions are event counts, so (0, 2) is the whole two-event log.
        var getEventsResult = await snackGrain.GetEventsAsync(0, 2);
        Assert.True(getEventsResult.IsSuccess);
        Assert.Collection(getEventsResult.Value,
            evt => Assert.Equal("Apple", Assert.IsType<SnackInitializedEvent>(evt).Name),
            evt => Assert.Equal("Orange", Assert.IsType<SnackNameChangedEvent>(evt).Name));

        foreach (var snackEvent in getEventsResult.Value)
        {
            _output.WriteLine(snackEvent.ToString());
        }
    }
}
