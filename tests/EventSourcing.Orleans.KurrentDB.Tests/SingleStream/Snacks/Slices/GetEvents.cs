using System.Collections.Immutable;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

public partial interface ISnackGrain : IEventDrivenGrain
{
    [ReadOnly]
    Task<Result<ImmutableList<SnackEvent>>> GetEventsAsync(int fromVersion, int toVersion);
}


public partial class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    /// <inheritdoc />
    public Task<Result<ImmutableList<SnackEvent>>> GetEventsAsync(int fromVersion, int toVersion)
    {
        return Result.Success()
            .MapTry(() => RetrieveConfirmedEvents(fromVersion, toVersion))
            .MapTry(list => list.ToImmutableList());
    }
}