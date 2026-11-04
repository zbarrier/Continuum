using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Transactions;
using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[GenerateSerializer, Immutable]
public sealed record TransferMoneyCommand(Guid ToAccountId, decimal Amount);

[GenerateSerializer, Immutable]
public sealed record PayAllCommand(TransferMoneyCommand[] Transfers);

[GenerateSerializer, Immutable]
public sealed class TransferMoneyResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[GenerateSerializer, Immutable]
public sealed class PayAllResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

[DomainEventType("Tests.Banking.MoneyDebitedEvent"), GenerateSerializer, Immutable]
public sealed record MoneyDebitedEvent(Guid AccountId, Guid TransferId, Guid ToAccountId, decimal Amount, DateTimeOffset OccurredAt)
    : BankAccountEvent(AccountId, OccurredAt);


public partial interface IBankAccountGrain : IEventDrivenGrain
{
    /// <summary>
    ///     Moves money from this account to another one. The debit here and the credit on the destination account are
    ///     saved together or not at all.
    /// </summary>
    Task<Result<TransferMoneyResponse>> TransferAsync(TransferMoneyCommand cmd);

    /// <summary>
    ///     Pays several transfers in one go, in the order given. Every transfer succeeds or none of them do. The same
    ///     destination account may appear more than once.
    /// </summary>
    Task<Result<PayAllResponse>> PayAllAsync(PayAllCommand cmd);
}


public partial class BankAccountGrain : EventDrivenGrain<BankAccountState, BankAccountEvent>, IBankAccountGrain
{
    /// <inheritdoc />
    public async Task<Result<TransferMoneyResponse>> TransferAsync(TransferMoneyCommand cmd)
    {
        return await PayAllAsync(new PayAllCommand([cmd]))
            .Map(response => new TransferMoneyResponse(response.Changes, response.Version));
    }

    /// <inheritdoc />
    public async Task<Result<PayAllResponse>> PayAllAsync(PayAllCommand cmd)
    {
        var fromAccountIdResult = AccountId.Create(this.GetPrimaryKeyString(), "AccountId");
        if (fromAccountIdResult.IsFailure)
        {
            return Result.Failure<PayAllResponse>(fromAccountIdResult.Error);
        }
        var fromAccountId = fromAccountIdResult.Value;

        var transfersResult = Result.NotNullOrEmpty(cmd.Transfers, "Transfers")
            .Bind(transfers => CreateAccountTransferCollection(fromAccountId, cmd.Transfers));
        if (transfersResult.IsFailure)
        {
            return Result.Failure<PayAllResponse>(transfersResult.Error);
        }
        var validatedTransfers = transfersResult.Value;

        var result = await Result.Success()
            .Ensure(() => State.IsOpen, RequestErrors.NewFailedPrecondition("The source account is not open."))
            .BindWithStreamTransaction(this, async transaction =>
            {
                for (int i = 0; i < validatedTransfers.Count; i++)
                {
                    var transfer = validatedTransfers[i];

                    // This account (the coordinator) takes the money out first, so the overdraft check sees earlier payments.
                    var debitedResult = Debit(transaction, transfer);
                    if (debitedResult.IsFailure)
                    {
                        return debitedResult;
                    }

                    // The destination account (a participant) puts the money in; only the events from this call come back.
                    var destination = GrainFactory.GetGrain<IBankAccountGrain>((string)transfer.ToAccountId).AsReference<ITransactionParticipant>();
                    var creditedResult = await transaction.PrepareAsync(destination, new CreditAccountCommand(transfer.TransferId, transfer.FromAccountId, transfer.Amount))
                        .Bind(preparedTransaction =>
                        {
                            // The coordinator checks that the destination credited exactly what was sent before moving on.
                            if (preparedTransaction.Events is not [MoneyCreditedEvent credit] || credit.TransferId != transfer.TransferId || credit.Amount != transfer.Amount)
                            {
                                return Result.Failure<PreparedTransaction>(RequestErrors.NewFailedPrecondition("The destination account did not prepare the expected credit event."));
                            }
                            return Result.Success(preparedTransaction);
                        });
                    if (creditedResult.IsFailure)
                    {
                        return creditedResult;
                    }
                }
                return Result.Success();
            })
            .Map(response => new PayAllResponse(response.Changes, response.Version));

        return result;
    }

    private Result<AccountTransferCollection> CreateAccountTransferCollection(AccountId fromAccountId, IEnumerable<TransferMoneyCommand> transfers)
    {
        var accountTransfers = new List<AccountTransfer>();
        var failedValidationResults = new List<Result<AccountTransfer>>();
        for (int i = 0; i < transfers.Count(); i++)
        {
            var transfer = transfers.ElementAt(i);
            var path = PropertyPath.Root.Append("Transfers").Index(i);

            var validationResult = Result.CombineForValidation(
                    TransferId.Create(Guid.NewGuid(), path, "TransferId").Deconstruct(out var transferId),
                    AccountId.Create(transfer.ToAccountId, path, "ToAccountId").Deconstruct(out var toAccountId),
                    TransferAmount.Create(transfer.Amount, path, "Amount").Deconstruct(out var amount)
                )
                .Bind(() => AccountTransfer.Create(transferId!, fromAccountId, toAccountId!, amount!, path))
                .Tap(accountTransfers.Add);
            if (validationResult.IsFailure)
            {
                failedValidationResults.Add(validationResult);
            }
        }
        if (failedValidationResults.Count > 0)
        {
            var failureResult = Result.CombineForValidation(failedValidationResults);
            return Result.Failure<AccountTransferCollection>(failureResult.Error);
        }
        return AccountTransferCollection.Create(accountTransfers, "Transfers");
    }

    private Result Debit(IStreamTransaction<BankAccountState, BankAccountEvent> transaction, AccountTransfer transfer)
    {
        return Result.Success()
            .Ensure(() => transfer.Amount <= transaction.State.AvailableFunds, RequestErrors.NewFailedPrecondition("The transfer would exceed the overdraft limit."))
            .Tap(() => transaction.Apply(new MoneyDebitedEvent(transfer.FromAccountId, transfer.TransferId, transfer.ToAccountId, transfer.Amount, DateTimeOffset.UtcNow)));
    }
}


public sealed partial class BankAccountState : EventDrivenState<BankAccountState, BankAccountEvent>
{
    public void Handle(MoneyDebitedEvent evt) => Balance -= evt.Amount;
}
