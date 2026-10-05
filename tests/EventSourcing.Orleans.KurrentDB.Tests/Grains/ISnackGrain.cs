using System.Collections.Immutable;

using Continuum.CSharpFunctionalExtensions;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Responses;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Grains;

public interface ISnackGrain : IGrainWithGuidKey
{
    [AlwaysInterleave]
    Task<Result<SnackState>> GetAsync();

    [AlwaysInterleave]
    Task<Result<ImmutableList<SnackEvent>>> GetEventsAsync(int fromVersion, int toVersion);

    [AlwaysInterleave]
    Task<bool> CanInitializeAsync();

    Task<Result<SnackInitializeResponse>> InitializeAsync(SnackInitializeCommand cmd);

    [AlwaysInterleave]
    Task<bool> CanRemoveAsync();

    Task<Result<SnackRemoveResponse>> RemoveAsync(SnackRemoveCommand cmd);

    [AlwaysInterleave]
    Task<bool> CanChangeNameAsync();

    Task<Result<SnackChangeNameResponse>> ChangeNameAsync(SnackChangeNameCommand cmd);
}
