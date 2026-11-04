using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[GenerateSerializer, Immutable]
public sealed record CloseAccountCommand;

[GenerateSerializer, Immutable]
public sealed class CloseAccountResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.Banking.BankAccountClosedEvent"), GenerateSerializer, Immutable]
public sealed record BankAccountClosedEvent(Guid AccountId, DateTimeOffset OccurredAt)
    : BankAccountEvent(AccountId, OccurredAt);


public partial interface IBankAccountGrain : IEventDrivenGrain
{
    Task<Result<CloseAccountResponse>> CloseAsync(CloseAccountCommand cmd);
}


public partial class BankAccountGrain : EventDrivenGrain<BankAccountState, BankAccountEvent>, IBankAccountGrain
{
    /// <inheritdoc />
    public async Task<Result<CloseAccountResponse>> CloseAsync(CloseAccountCommand cmd)
    {
        var result = await Result.CombineForValidation(
                AccountId.Create(this.GetPrimaryKeyString(), "AccountId").Deconstruct(out var accountId)
            )
            .Ensure(() => State.IsOpen, () => RequestErrors.NewFailedPrecondition("The account is not open."))
            .BindWithStreamTransaction(this, transaction =>
            {
                transaction.Apply(new BankAccountClosedEvent(accountId!, DateTimeOffset.UtcNow));
            })
            .Map(response => new CloseAccountResponse(response.Changes, response.Version));

        return result;
    }
}


public sealed partial class BankAccountState : EventDrivenState<BankAccountState, BankAccountEvent>
{
    public void Handle(BankAccountClosedEvent evt) => IsOpen = false;
}
