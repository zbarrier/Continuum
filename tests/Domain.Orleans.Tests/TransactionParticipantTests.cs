using Continuum.CSharpFunctionalExtensions;
using Continuum.Errors;

using Orleans.EventSourcing;
using Orleans.Providers;

namespace Continuum.Domain.Orleans.Tests;

[GenerateSerializer]
public sealed record IncrementCounter([property: Id(0)] int By);

[GenerateSerializer]
public sealed record IncrementWithoutVersionCheck([property: Id(0)] int By);

[GenerateSerializer]
public sealed record ResetCounter;

public interface IParticipantCounterGrain : ITransactionParticipant
{
    Task<Result<IStreamTransactionResponse>> Increment(IncrementCounter command);
    Task<Result<IStreamTransactionResponse>> Reset(ResetCounter command);
    Task<int> GetCount();
    Task<int> GetVersion();
    Task AppendDirectly(params CounterEvent[] events);
}

[LogConsistencyProvider(ProviderName = GrainTestSiloConfigurator.LogStorage)]
[StorageProvider(ProviderName = GrainTestSiloConfigurator.Storage)]
public sealed class ParticipantCounterGrain : EventDrivenGrain<CounterState, CounterEvent>, IParticipantCounterGrain
{
    public ParticipantCounterGrain(IServiceProvider serviceProvider)
        : base(serviceProvider, GrainTestSiloConfigurator.TypeMapperKey)
    {
        OnTransaction<IncrementCounter>(IncrementCore);
        OnTransaction<IncrementWithoutVersionCheck>((command, transaction) => IncrementCore(new IncrementCounter(command.By), transaction),
            checkVersion: false);
    }

    // Supports coordinated transactions: the logic lives in a core method shared with the transaction handler.
    public async Task<Result<IStreamTransactionResponse>> Increment(IncrementCounter command)
    {
        await using var transaction = BeginStreamTransaction();
        return await IncrementCore(command, transaction)
            .Bind(() => SaveChanges(transaction));
    }

    // Normal-only command: no core method and no transaction registration.
    public async Task<Result<IStreamTransactionResponse>> Reset(ResetCounter command)
    {
        await using var transaction = BeginStreamTransaction();
        transaction.Apply(new CounterReset());
        return await SaveChanges(transaction);
    }

    public Task<int> GetCount() => Task.FromResult(State.Count);
    public Task<int> GetVersion() => Task.FromResult(Version);

    // Stands in for the coordinator's append until the adaptor supports multi-stream appends.
    public async Task AppendDirectly(params CounterEvent[] events) => await RaiseConditionalEvents(events);

    private static Task<Result> IncrementCore(IncrementCounter command, IStreamTransaction<CounterState, CounterEvent> transaction)
    {
        if (command.By <= 0)
        {
            return Task.FromResult(Result.Failure(ContinuumErrors.NewOptimisticConcurrencyConflict("Must be positive.")));
        }
        transaction.Apply(new CounterIncremented(command.By));
        return Task.FromResult(Result.Success());
    }
}

public class TransactionParticipantTests(GrainClusterFixture fixture) : IClassFixture<GrainClusterFixture>
{
    private IParticipantCounterGrain NewGrain() => fixture.Cluster.GrainFactory.GetGrain<IParticipantCounterGrain>(Guid.NewGuid().ToString());

    [Fact]
    public async Task PrepareAsync_ReturnsEventsAndVersion_WithoutChangingState()
    {
        var grain = NewGrain();
        await grain.AppendDirectly(new CounterIncremented(1));

        var result = await grain.PrepareAsync(Guid.NewGuid(), new IncrementCounter(5), TimeSpan.FromSeconds(10));

        Assert.True(result.IsSuccess);
        Assert.Equal(grain.GetGrainId(), result.Value.GrainId);
        Assert.Equal(new CounterIncremented(5), Assert.Single(result.Value.Events));
        Assert.Equal(1, result.Value.ExpectedVersion);
        Assert.Equal(1, await grain.GetCount());
        Assert.Equal(1, await grain.GetVersion());
    }

    [Fact]
    public async Task Increment_NormalCall_CommitsEvents()
    {
        var grain = NewGrain();

        var result = await grain.Increment(new IncrementCounter(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Version);
        Assert.Equal(3, await grain.GetCount());
        Assert.Equal(1, await grain.GetVersion());
    }

    [Fact]
    public async Task PrepareAsync_WithoutVersionCheck_ReturnsNullExpectedVersion()
    {
        var grain = NewGrain();

        var result = await grain.PrepareAsync(Guid.NewGuid(), new IncrementWithoutVersionCheck(2), TimeSpan.FromSeconds(10));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ExpectedVersion);
    }

    [Fact]
    public async Task PrepareAsync_ReturnsHandlerFailure()
    {
        var grain = NewGrain();

        var result = await grain.PrepareAsync(Guid.NewGuid(), new IncrementCounter(0), TimeSpan.FromSeconds(10));

        Assert.True(result.IsFailure);
        Assert.Equal(ContinuumErrorCodes.OptimisticConcurrencyConflict, ((RequestError)result.Error).Code);
    }

    [Fact]
    public async Task PrepareAsync_NormalOnlyCommand_Throws()
    {
        var grain = NewGrain();

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            grain.PrepareAsync(Guid.NewGuid(), new ResetCounter(), TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public async Task CommitAsync_RefreshesFromTheLog()
    {
        var grain = NewGrain();
        var transactionId = Guid.NewGuid();
        await grain.PrepareAsync(transactionId, new IncrementCounter(5), TimeSpan.FromSeconds(10));
        await grain.AppendDirectly(new CounterIncremented(5));

        await grain.CommitAsync(transactionId);

        Assert.Equal(5, await grain.GetCount());
        Assert.Equal(1, await grain.GetVersion());
    }

    [Fact]
    public async Task RollbackAsync_LeavesStateUnchanged()
    {
        var grain = NewGrain();
        var transactionId = Guid.NewGuid();
        await grain.PrepareAsync(transactionId, new IncrementCounter(5), TimeSpan.FromSeconds(10));

        await grain.RollbackAsync(transactionId);

        Assert.Equal(0, await grain.GetCount());
        Assert.Equal(0, await grain.GetVersion());
    }
}
