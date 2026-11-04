using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Snacks;

public partial interface ISnackGrain : IEventDrivenGrain
{
    [ReadOnly]
    Task<Result<SnackState>> GetAsync();
}


public partial class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    /// <inheritdoc />
    public Task<Result<SnackState>> GetAsync()
    {
        return Task.FromResult(
            Result.Success(State)
                .Ensure(snack => snack.IsCreated, RequestErrors.NewFailedPrecondition("The snack is not initialized."))
            );
    }
}