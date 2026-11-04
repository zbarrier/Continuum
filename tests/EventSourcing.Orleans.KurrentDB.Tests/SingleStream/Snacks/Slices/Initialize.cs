using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

[GenerateSerializer, Immutable]
public sealed record SnackInitializeCommand(string Name);

[GenerateSerializer, Immutable]
public sealed class SnackInitializeResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.SnackInitializedEvent"), GenerateSerializer, Immutable]
public sealed record SnackInitializedEvent(Guid Id, string Name, DateTimeOffset OccurredAt)
    : SnackEvent(Id, OccurredAt);


public partial interface ISnackGrain : IEventDrivenGrain
{
    Task<Result<SnackInitializeResponse>> InitializeAsync(SnackInitializeCommand cmd);
}


public partial class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    /// <inheritdoc />
    public async Task<Result<SnackInitializeResponse>> InitializeAsync(SnackInitializeCommand cmd)
    {
        return await Result.CombineForValidation(
                SnackId.Create(this.GetPrimaryKeyString(), "SnackId").Deconstruct(out var snackId),
                SnackName.Create(cmd.Name, "Name").Deconstruct(out var name)
            )
            .Ensure(() => !State.IsCreated, () => RequestErrors.NewFailedPrecondition("The snack already exists."))
            .BindWithStreamTransaction(this, transaction =>
            {
                transaction.Apply(new SnackInitializedEvent(snackId!, name!, DateTimeOffset.UtcNow));
            })
            .Map(response => new SnackInitializeResponse(response.Changes, response.Version));
    }
}


public sealed partial class SnackState : EventDrivenState<SnackState, SnackEvent>
{
    public void Handle(SnackInitializedEvent evt)
    {
        Id = evt.Id;
        Name = evt.Name;
        CreatedAt = evt.OccurredAt;
    }
}