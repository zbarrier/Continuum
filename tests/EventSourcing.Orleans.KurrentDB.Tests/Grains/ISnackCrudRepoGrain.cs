using System.Collections.Immutable;

using Continuum.CSharpFunctionalExtensions;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Grains;

public interface ISnackCrudRepoGrain : IGrainWithGuidKey
{
    [AlwaysInterleave]
    Task<Result<ISnackGrain>> GetAsync(SnackRepoGetCommand command);

    [AlwaysInterleave]
    Task<Result<ImmutableList<ISnackGrain>>> GetManyAsync(SnackRepoGetManyCommand command);

    Task<Result> CreateAsync(SnackRepoCreateCommand cmd);

    Task<Result> DeleteAsync(SnackRepoDeleteCommand cmd);
}
