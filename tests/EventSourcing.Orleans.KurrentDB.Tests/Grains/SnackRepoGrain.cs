using System.Collections.Immutable;

using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;

using Microsoft.Extensions.Logging;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Grains;

[StatelessWorker]
public class SnackRepoGrain : Grain, ISnackCrudRepoGrain
{
    private readonly ILogger<SnackRepoGrain> _logger;
    private readonly Func<Exception, Error> _grainCallErrorHandler;

    /// <inheritdoc />
    public SnackRepoGrain(ILogger<SnackRepoGrain> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _grainCallErrorHandler = ex =>
        {
            return GlobalGrainErrorHandler.Default(ex, _logger);
        };
    }

    /// <inheritdoc />
    public Task<Result<ISnackGrain>> GetAsync(SnackRepoGetCommand cmd)
    {
        return Task.FromResult(Result.Success(GrainFactory.GetGrain<ISnackGrain>(cmd.Id)));
    }

    /// <inheritdoc />
    public Task<Result<ImmutableList<ISnackGrain>>> GetManyAsync(SnackRepoGetManyCommand cmd)
    {
        var snacks = cmd.Ids.Select(id => GrainFactory.GetGrain<ISnackGrain>(id));
        return Task.FromResult(Result.Success(snacks.ToImmutableList()));
    }

    /// <inheritdoc />
    public async Task<Result> CreateAsync(SnackRepoCreateCommand cmd)
    {
        return await Result.Success()
            .MapTry(() => GrainFactory.GetGrain<ISnackGrain>(cmd.Id))
            .Ensure(grain => grain.CanInitializeAsync(), RequestErrors.NewFailedPrecondition("Snack {0} already exists or has been deleted.", cmd.Id))
            .BindTry(grain => 
                grain.InitializeAsync(new SnackInitializeCommand(cmd.Name, cmd.TraceId, DateTimeOffset.UtcNow, cmd.OperatedBy)), 
                _grainCallErrorHandler);
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(SnackRepoDeleteCommand cmd)
    {
        return await Result.Success()
            .MapTry(() => GrainFactory.GetGrain<ISnackGrain>(cmd.Id))
            .Ensure(grain => grain.CanRemoveAsync(), RequestErrors.NewFailedPrecondition("Snack {0} does not exists or has been deleted.", cmd.Id))
            .BindTry(grain => 
                grain.RemoveAsync(new SnackRemoveCommand(cmd.TraceId, DateTimeOffset.UtcNow, cmd.OperatedBy)), 
                _grainCallErrorHandler);
    }
}
