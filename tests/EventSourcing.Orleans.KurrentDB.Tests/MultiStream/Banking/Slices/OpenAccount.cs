using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[GenerateSerializer, Immutable]
public sealed record OpenAccountCommand(decimal OpeningBalance, decimal OverdraftLimit);

[GenerateSerializer, Immutable]
public sealed class OpenAccountResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.Banking.BankAccountOpenedEvent"), GenerateSerializer, Immutable]
public sealed record BankAccountOpenedEvent(Guid AccountId, decimal OpeningBalance, decimal OverdraftLimit, DateTimeOffset OccurredAt)
    : BankAccountEvent(AccountId, OccurredAt);


public partial interface IBankAccountGrain : IEventDrivenGrain
{
    Task<Result<OpenAccountResponse>> OpenAsync(OpenAccountCommand cmd);
}


public partial class BankAccountGrain : EventDrivenGrain<BankAccountState, BankAccountEvent>, IBankAccountGrain
{
    /// <inheritdoc />
    public async Task<Result<OpenAccountResponse>> OpenAsync(OpenAccountCommand cmd)
    {
        var result = await Result.CombineForValidation(
                AccountId.Create(this.GetPrimaryKeyString(), "AccountId").Deconstruct(out var accountId),
                OpeningBalance.Create(cmd.OpeningBalance, "OpeningBalance").Deconstruct(out var openingBalance),
                OverDraftLimit.Create(cmd.OverdraftLimit, "OverdraftLimit").Deconstruct(out var overdraftLimit)
            )
            .Ensure(() => !State.IsOpen, () => RequestErrors.NewFailedPrecondition("The account is already open."))
            .BindWithStreamTransaction(this, transaction => 
            {
                transaction.Apply(new BankAccountOpenedEvent(accountId!, openingBalance!, overdraftLimit!, DateTimeOffset.UtcNow));
            })
            .Map(response => new OpenAccountResponse(response.Changes, response.Version));

        return result;
    }
}


public sealed partial class BankAccountState : EventDrivenState<BankAccountState, BankAccountEvent>
{
    public void Handle(BankAccountOpenedEvent evt)
    {
        Id = evt.AccountId;
        IsOpen = true;
        Balance = evt.OpeningBalance;
        OverdraftLimit = evt.OverdraftLimit;
    }
}
