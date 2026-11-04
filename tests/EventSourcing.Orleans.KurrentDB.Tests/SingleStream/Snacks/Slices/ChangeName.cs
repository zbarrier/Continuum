using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

[GenerateSerializer, Immutable]
public sealed record SnackChangeNameCommand(string Name);

[GenerateSerializer, Immutable]
public sealed class SnackChangeNameResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.SnackNameChangedEvent"), GenerateSerializer, Immutable]
public sealed record SnackNameChangedEvent(Guid Id, string Name, DateTimeOffset OccurredAt)
    : SnackEvent(Id, OccurredAt);


public partial interface ISnackGrain : IEventDrivenGrain
{
    Task<Result<SnackChangeNameResponse>> ChangeNameAsync(SnackChangeNameCommand cmd);
}


public partial class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    /// <inheritdoc />
    public async Task<Result<SnackChangeNameResponse>> ChangeNameAsync(SnackChangeNameCommand cmd)
    {
        return await Result.CombineForValidation(
                SnackId.Create(this.GetPrimaryKeyString(), "SnackId").Deconstruct(out var snackId),
                SnackName.Create(cmd.Name, "Name").Deconstruct(out var name)
            )
            .Ensure(() => State.IsCreated, () => RequestErrors.NewFailedPrecondition("The snack is not initialized."))
            .Ensure(() => !State.IsDeleted, () => RequestErrors.NewFailedPrecondition("The snack has already been removed."))
            .BindWithStreamTransaction(this, transaction =>
            {
                if (State.Name == name!)
                {
                    return;
                }
                transaction.Apply(new SnackNameChangedEvent(snackId!, name!, DateTimeOffset.UtcNow));
            })
            .Map(response => new SnackChangeNameResponse(response.Changes, response.Version));
    }
}


public sealed partial class SnackState : EventDrivenState<SnackState, SnackEvent>
{
    public void Handle(SnackNameChangedEvent evt)
    {
        Name = evt.Name;
        LastModifiedAt = evt.OccurredAt;
    }
}