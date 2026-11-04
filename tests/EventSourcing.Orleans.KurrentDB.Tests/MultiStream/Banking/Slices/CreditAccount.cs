using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

/// <summary>
///     Asks a destination account to accept money sent from another account as part of a transfer.
/// </summary>
[GenerateSerializer, Immutable]
public sealed record CreditAccountCommand(Guid TransferId, Guid FromAccountId, decimal Amount);

[GenerateSerializer, Immutable]
public sealed class CreditAccountResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.Banking.MoneyCreditedEvent"), GenerateSerializer, Immutable]
public sealed record MoneyCreditedEvent(Guid AccountId, Guid TransferId, Guid FromAccountId, decimal Amount, DateTimeOffset OccurredAt)
    : BankAccountEvent(AccountId, OccurredAt);


public partial interface IBankAccountGrain : IEventDrivenGrain
{
    Task<Result<CreditAccountResponse>> CreditAsync(CreditAccountCommand cmd);
}


public partial class BankAccountGrain : EventDrivenGrain<BankAccountState, BankAccountEvent>, IBankAccountGrain
{
    public async Task<Result<CreditAccountResponse>> CreditAsync(CreditAccountCommand cmd)
    {
        var result = await Result.Success()
            .BindWithStreamTransaction(this, transaction => CreditAsyncCore(cmd, transaction))
            .Map(response => new CreditAccountResponse(response.Changes, response.Version));

        return result;
    }

    /// <summary>
    ///     Runs when another account includes this one in its transfer. The events are returned to the coordinating
    ///     account, which saves them; this account does not save them itself.
    /// </summary>
    private Task<Result> CreditAsyncCore(CreditAccountCommand cmd, IStreamTransaction<BankAccountState, BankAccountEvent> transaction)
    {
        var result = Result.CombineForValidation(
                AccountId.Create(this.GetPrimaryKeyString(), "AccountId").Deconstruct(out var accountId),
                TransferId.Create(cmd.TransferId, "TransferId").Deconstruct(out var transferId),
                AccountId.Create(cmd.FromAccountId, "FromAccountId").Deconstruct(out var fromAccountId),
                CreditAmount.Create(cmd.Amount, "Amount").Deconstruct(out var amount)
            )
            .Ensure(() => State.IsOpen, () => RequestErrors.NewFailedPrecondition("The destination account is not open."))
            .Tap(() => transaction.Apply(new MoneyCreditedEvent(accountId!, transferId!, fromAccountId!, amount!, DateTimeOffset.UtcNow)));

        return Task.FromResult(result);
    }
}


public sealed partial class BankAccountState : EventDrivenState<BankAccountState, BankAccountEvent>
{
    public void Handle(MoneyCreditedEvent evt) => Balance += evt.Amount;
}
