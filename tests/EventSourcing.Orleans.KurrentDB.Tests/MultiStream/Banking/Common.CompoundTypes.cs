using Continuum.CSharpFunctionalExtensions;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[Alias("Tests.AccountTransfer"), GenerateSerializer, Immutable]
public sealed record AccountTransfer
{
    public static Result<AccountTransfer> Create(TransferId transferId, AccountId fromAccountId, AccountId toAccountId, TransferAmount amount, PropertyPath path)
    {
        // All entries in CombineForValidation are evaluated.  We want all errors to be reported, so we use CombineForValidation instead of Combine.
        // Use multiple stages of CombineForValidation to evaluate rules that depend on multiple properties, but only if the first stage passes.
        // This allows us to avoid null reference exceptions when evaluating rules that depend on multiple properties.
        return Result.CombineForValidation(
                Result.NotNull(transferId, path, "TransferId"),
                Result.NotNull(fromAccountId, path, "FromAccountId"),
                Result.NotNull(toAccountId, path, "ToAccountId"),
                Result.NotNull(amount, path, "Amount")
            )
            .CombineForValidation(
                Result.NotEqual(fromAccountId, toAccountId, path, "FromAccountId", "ToAccountId")
            )
            .Map(() => new AccountTransfer(transferId, fromAccountId, toAccountId, amount));
    }

    private AccountTransfer(TransferId transferId, AccountId fromAccountId, AccountId toAccountId, TransferAmount amount)
    {
        TransferId = transferId;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
    }

    [Id(0)] public TransferId TransferId { get; }
    [Id(1)] public AccountId FromAccountId { get; }
    [Id(2)] public AccountId ToAccountId { get; }
    [Id(3)] public TransferAmount Amount { get; }
}

[Alias("Tests.AccountTransferCollection"), GenerateSerializer, Immutable]
public sealed record AccountTransferCollection
{
    public static Result<AccountTransferCollection> Create(IReadOnlyList<AccountTransfer> transfers, string propertyName)
    {
        return Result.NotNullOrEmpty(transfers, propertyName)
            .Map(_ => new AccountTransferCollection(transfers));
    }

    private AccountTransferCollection(IReadOnlyList<AccountTransfer> transfers)
    {
        _transfers = transfers.ToArray();
    }

    [Id(0)] private readonly IReadOnlyList<AccountTransfer> _transfers;

    public AccountTransfer this[int index] => _transfers[index];

    public int Count => _transfers.Count;
}