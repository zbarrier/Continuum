using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

using Microsoft.Extensions.DependencyInjection;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream;

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
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId.ToString());
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple"));
        Assert.True(initializeResult.IsSuccess);
        Assert.Equal(1, initializeResult.Value.Version);
        Assert.IsType<SnackInitializedEvent>(Assert.Single(initializeResult.Value.Changes).Event);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Should_Not_Create_A_Snack_With_An_Invalid_Name(string name)
    {
        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(Guid.NewGuid().ToString());

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand(name));
        Assert.True(initializeResult.IsFailure);
    }

    [Fact]
    public async Task Should_Change_Name_Of_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId.ToString());
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple"));
        Assert.True(initializeResult.IsSuccess);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Orange"));
        Assert.True(changeNameResult.IsSuccess);
        Assert.Equal(2, changeNameResult.Value.Version);
        Assert.IsType<SnackNameChangedEvent>(Assert.Single(changeNameResult.Value.Changes).Event);
    }

    [Fact]
    public async Task Should_Remove_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId.ToString());
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple"));
        Assert.True(initializeResult.IsSuccess);

        var removeResult = await snackGrain.RemoveAsync(new SnackRemoveCommand());
        Assert.True(removeResult.IsSuccess);
        Assert.Equal(2, removeResult.Value.Version);
        Assert.IsType<SnackRemovedEvent>(Assert.Single(removeResult.Value.Changes).Event);

        var secondRemoveResult = await snackGrain.RemoveAsync(new SnackRemoveCommand());
        Assert.True(secondRemoveResult.IsFailure);
    }

    [Fact]
    public async Task Should_Get_Snack()
    {
        var snackId = Guid.NewGuid();

        var grainFactory = _fixture.Cluster.ServiceProvider.GetRequiredService<IGrainFactory>();
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId.ToString());
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple"));
        Assert.True(initializeResult.IsSuccess);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Orange"));
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
        var snackGrain = grainFactory.GetGrain<ISnackGrain>(snackId.ToString());
        Assert.NotNull(snackGrain);

        var initializeResult = await snackGrain.InitializeAsync(new SnackInitializeCommand("Apple"));
        Assert.True(initializeResult.IsSuccess);

        var changeNameResult = await snackGrain.ChangeNameAsync(new SnackChangeNameCommand("Orange"));
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
