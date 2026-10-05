using System.Collections.Immutable;

using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Commands;
using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;
using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Responses;

using Microsoft.Extensions.Logging;

using Orleans.Providers;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Grains;

[LogConsistencyProvider(ProviderName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)]
[StorageProvider(ProviderName = "MemoryStorageProvider")]
public class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    private readonly ILogger<SnackGrain> _logger;
    private readonly Func<Exception, Error> _grainCallErrorHandler;

    /// <inheritdoc />
    public SnackGrain(IServiceProvider serviceProvider, ILogger<SnackGrain> logger)
        : base(serviceProvider, "journaledGrainLog")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _grainCallErrorHandler = ex =>
        {
            return GlobalGrainErrorHandler.Default(ex, _logger);
        };
    }

    /// <inheritdoc />
    public Task<Result<SnackState>> GetAsync()
    {
        var id = this.GetPrimaryKey();
        return Task.FromResult(
            Result.Success(State)
                .Ensure(snack => snack.IsCreated, RequestErrors.NewFailedPrecondition("Snack {0} is not initialized.", id))
            );
    }

    /// <inheritdoc />
    public Task<Result<ImmutableList<SnackEvent>>> GetEventsAsync(int fromVersion, int toVersion)
    {
        return Result.Success()
            .MapTry(() => RetrieveConfirmedEvents(fromVersion, toVersion))
            .MapTry(list => list.ToImmutableList());
    }

    /// <inheritdoc />
    public Task<bool> CanInitializeAsync()
    {
        return Task.FromResult(State.IsDeleted == false && State.IsCreated == false);
    }

    /// <inheritdoc />
    public async Task<Result<SnackInitializeResponse>> InitializeAsync(SnackInitializeCommand cmd)
    {
        var id = this.GetPrimaryKey();
        return await Result.Success()
            .Ensure(() => !State.IsDeleted, RequestErrors.NewFailedPrecondition("Snack {0} has already been removed.", id))
            .Ensure(() => !State.IsCreated, RequestErrors.NewFailedPrecondition("Snack {0} already exists.", id))
            .Ensure(() => State.Name.Length <= 100, RequestErrors.NewFailedPrecondition("The name of snack {0} is too long.", id))
            .BindWithSessionScope(this, session =>
            {
                session.Apply(new SnackInitializedEvent(id, cmd.Name, cmd.TraceId, DateTimeOffset.UtcNow, cmd.OperatedBy, Version));
            })
            .Map(response => new SnackInitializeResponse(response.Changes, response.Version));
    }

    /// <inheritdoc />
    public Task<bool> CanRemoveAsync()
    {
        return Task.FromResult(State.IsDeleted == false && State.IsCreated);
    }

    /// <inheritdoc />
    public async Task<Result<SnackRemoveResponse>> RemoveAsync(SnackRemoveCommand cmd)
    {
        var id = this.GetPrimaryKey();
        return await Result.Success()
            .Ensure(() => !State.IsDeleted, RequestErrors.NewFailedPrecondition("Snack {0} has already been removed.", id))
            .Ensure(() => State.IsCreated, RequestErrors.NewFailedPrecondition("Snack {0} is not initialized.", id))
            .BindWithSessionScope(this, session =>
            {
                session.Apply(new SnackRemovedEvent(id, cmd.TraceId, DateTimeOffset.UtcNow, cmd.OperatedBy, Version));
            })
            .Map(response => new SnackRemoveResponse(response.Changes, response.Version));
    }

    /// <inheritdoc />
    public Task<bool> CanChangeNameAsync()
    {
        return Task.FromResult(State.IsDeleted == false && State.IsCreated);
    }

    /// <inheritdoc />
    public async Task<Result<SnackChangeNameResponse>> ChangeNameAsync(SnackChangeNameCommand cmd)
    {
        var id = this.GetPrimaryKey();
        return await Result.Success()
            .Ensure(() => !State.IsDeleted, RequestErrors.NewFailedPrecondition("Snack {0} has already been removed.", id))
            .Ensure(() => State.IsCreated, RequestErrors.NewFailedPrecondition("Snack {0} is not initialized.", id))
            .Ensure(() => State.Name.Length <= 100, RequestErrors.NewFailedPrecondition("The name of snack {0} is too long.", id))
            .BindWithSessionScope(this, session =>
            {
                session.Apply(new SnackNameChangedEvent(id, cmd.Name, cmd.TraceId, DateTimeOffset.UtcNow, cmd.OperatedBy, Version));
            })
            .Map(response => new SnackChangeNameResponse(response.Changes, response.Version));
    }
}
