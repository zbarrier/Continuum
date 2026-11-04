using Continuum.Domain.Orleans;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

public partial interface IBankAccountGrain : IEventDrivenGrain
{
    [ReadOnly]
    Task<BankAccountState> GetStateAsync();
}


public partial class BankAccountGrain : EventDrivenGrain<BankAccountState, BankAccountEvent>, IBankAccountGrain
{
    /// <inheritdoc />
    public Task<BankAccountState> GetStateAsync() => Task.FromResult(State);
}
