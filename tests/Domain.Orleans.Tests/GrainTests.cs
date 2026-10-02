using Continuum.CSharpFunctionalExtensions;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.TestingHost;

namespace Continuum.Domain.Orleans.Tests;

public interface ICounterGrain : IEventDrivenGrain
{
    Task<Result<ISaveChangesResponse>> Increment(int by, bool failBeforeSave);
    Task<int> GetCount();
    Task<int> GetVersion();
}

public interface IDoorGrain : IEventDrivenStateMachineGrain
{
    Task<Result<ISaveChangesResponse>> Open(bool failBeforeSave);
    Task<DoorStatus> GetStatus();
    Task<int> GetOpenCount();
}

[LogConsistencyProvider(ProviderName = GrainTestSiloConfigurator.LogStorage)]
[StorageProvider(ProviderName = GrainTestSiloConfigurator.Storage)]
public sealed class CounterGrain(IServiceProvider serviceProvider)
    : EventDrivenGrain<CounterState, CounterEvent>(serviceProvider, GrainTestSiloConfigurator.TypeMapperKey), ICounterGrain
{
    public async Task<Result<ISaveChangesResponse>> Increment(int by, bool failBeforeSave)
    {
        using var session = CreateSession();
        session.Apply(new CounterIncremented(by));
        if (failBeforeSave)
        {
            throw new InvalidOperationException("Simulated failure before save.");
        }
        return await SaveChanges(session);
    }

    public Task<int> GetCount() => Task.FromResult(State.Count);
    public Task<int> GetVersion() => Task.FromResult(Version);
}

[LogConsistencyProvider(ProviderName = GrainTestSiloConfigurator.LogStorage)]
[StorageProvider(ProviderName = GrainTestSiloConfigurator.Storage)]
public sealed class DoorGrain(IServiceProvider serviceProvider)
    : EventDrivenStateMachineGrain<DoorState, DoorEvent, DoorStatus, DoorCommand>(serviceProvider, GrainTestSiloConfigurator.TypeMapperKey), IDoorGrain
{
    public async Task<Result<ISaveChangesResponse>> Open(bool failBeforeSave)
    {
        using var session = CreateSession();
        session.Apply(new DoorOpened());
        if (failBeforeSave)
        {
            throw new InvalidOperationException("Simulated failure before save.");
        }
        return await SaveChanges(session);
    }

    public Task<DoorStatus> GetStatus() => Task.FromResult(State.StateMachine.State);
    public Task<int> GetOpenCount() => Task.FromResult(State.OpenCount);
}

public sealed class GrainTestSiloConfigurator : ISiloConfigurator
{
    public const string LogStorage = "LogStorage";
    public const string Storage = "Default";
    public const string TypeMapperKey = "test";

    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorage(Storage);
        siloBuilder.AddLogStorageBasedLogConsistencyProvider(LogStorage);
        siloBuilder.Services.AddKeyedSingleton<ITypeMapper>(TypeMapperKey, (_, _) =>
        {
            var mapper = new TestTypeMapper();
            mapper.AddType(typeof(DoorOpened), "DoorOpened");
            return mapper;
        });
    }
}

public sealed class GrainClusterFixture : IAsyncLifetime
{
    public TestCluster Cluster { get; } = new TestClusterBuilder(1)
        .AddSiloBuilderConfigurator<GrainTestSiloConfigurator>()
        .Build();

    public async ValueTask InitializeAsync() => await Cluster.DeployAsync();

    public async ValueTask DisposeAsync() => await Cluster.StopAllSilosAsync();
}

public class GrainTests(GrainClusterFixture fixture) : IClassFixture<GrainClusterFixture>
{
    private IGrainFactory Grains => fixture.Cluster.GrainFactory;

    [Fact]
    public async Task EventDrivenGrain_SuccessfulSave_AppliesEventsAndReturnsChanges()
    {
        var grain = Grains.GetGrain<ICounterGrain>(Guid.NewGuid().ToString());

        var result = await grain.Increment(3, failBeforeSave: false);

        Assert.True(result.IsSuccess);
        var change = Assert.Single(result.Value.Changes);
        Assert.Equal("CounterIncremented", change.EventType);
        Assert.Equal(1, result.Value.Version);
        Assert.Equal(3, await grain.GetCount());
        Assert.Equal(1, await grain.GetVersion());
    }

    [Fact]
    public async Task EventDrivenGrain_FailureBeforeSave_LeavesGrainStateUnchanged()
    {
        var grain = Grains.GetGrain<ICounterGrain>(Guid.NewGuid().ToString());
        await grain.Increment(2, failBeforeSave: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => grain.Increment(5, failBeforeSave: true));

        Assert.Equal(2, await grain.GetCount());
        Assert.Equal(1, await grain.GetVersion());
    }

    [Fact]
    public async Task EventDrivenGrain_LoadGivenEvents_SetsInitialState()
    {
        var grain = Grains.GetGrain<ICounterGrain>(Guid.NewGuid().ToString());

        await grain.LoadGivenEvents(new CounterIncremented(4), new CounterIncremented(6));

        Assert.Equal(10, await grain.GetCount());
    }

    [Fact]
    public async Task EventDrivenStateMachineGrain_SuccessfulSave_TransitionsGrainState()
    {
        var grain = Grains.GetGrain<IDoorGrain>(Guid.NewGuid().ToString());

        var result = await grain.Open(failBeforeSave: false);

        Assert.True(result.IsSuccess);
        Assert.Equal("DoorOpened", Assert.Single(result.Value.Changes).EventType);
        Assert.Equal(DoorStatus.Open, await grain.GetStatus());
        Assert.Equal(1, await grain.GetOpenCount());
    }

    [Fact]
    public async Task EventDrivenStateMachineGrain_FailureBeforeSave_LeavesGrainStateUnchanged()
    {
        var grain = Grains.GetGrain<IDoorGrain>(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<InvalidOperationException>(() => grain.Open(failBeforeSave: true));

        Assert.Equal(DoorStatus.Closed, await grain.GetStatus());
        Assert.Equal(0, await grain.GetOpenCount());
    }
}
