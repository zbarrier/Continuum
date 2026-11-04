using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Snacks;

[GenerateSerializer, Immutable]
public sealed record SnackRemoveCommand;

[GenerateSerializer, Immutable]
public sealed class SnackRemoveResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.SnackRemovedEvent"), GenerateSerializer, Immutable]
public sealed record SnackRemovedEvent(Guid Id, DateTimeOffset OccurredAt)
    : SnackEvent(Id, OccurredAt);


public partial interface ISnackGrain : IEventDrivenGrain
{
    Task<Result<SnackRemoveResponse>> RemoveAsync(SnackRemoveCommand cmd);
}


public partial class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    /// <inheritdoc />
    public async Task<Result<SnackRemoveResponse>> RemoveAsync(SnackRemoveCommand cmd)
    {
        return await Result.CombineForValidation(
                SnackId.Create(this.GetPrimaryKeyString(), "SnackId").Deconstruct(out var snackId)
            )
            .Ensure(() => State.IsCreated, () => RequestErrors.NewFailedPrecondition("The snack is not initialized."))
            .Ensure(() => !State.IsDeleted, () => RequestErrors.NewFailedPrecondition("The snack has already been removed."))
            .BindWithStreamTransaction(this, transaction =>
            {
                transaction.Apply(new SnackRemovedEvent(snackId!, DateTimeOffset.UtcNow));
            })
            .Map(response => new SnackRemoveResponse(response.Changes, response.Version));
    }
}


public sealed partial class SnackState : EventDrivenState<SnackState, SnackEvent>
{
    public void Handle(SnackRemovedEvent evt)
    {
        DeletedAt = evt.OccurredAt;
        IsDeleted = true;
    }
}