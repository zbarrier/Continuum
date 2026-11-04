using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[Alias("Tests.AccountId"), GenerateSerializer, Immutable]
public sealed record AccountId : GuidValueObject<AccountId>
{
    public static Result<AccountId> Create(string? value, string propertyName = nameof(AccountId))
        => Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new AccountId(validatedValue));
    public static Result<AccountId> Create(string? value, PropertyPath path, string member)
        => Result.IsGuidAndNotEmpty(value, path, member)
            .Map(validatedValue => new AccountId(validatedValue));
    public static Result<AccountId> Create(Guid value, string propertyName = nameof(AccountId))
        => Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new AccountId(validatedValue));
    public static Result<AccountId> Create(Guid value, PropertyPath path, string member)
        => Result.NotEmpty(value, path, member)
            .Map(validatedValue => new AccountId(validatedValue));

    private AccountId(Guid value) : base(value) { }
}

[Alias("Tests.TransferId"), GenerateSerializer, Immutable]
public sealed record TransferId : GuidValueObject<TransferId>
{
    public static Result<TransferId> Create(string? value, string propertyName = nameof(TransferId))
        => Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new TransferId(validatedValue));
    public static Result<TransferId> Create(string? value, PropertyPath path, string member)
        => Result.IsGuidAndNotEmpty(value, path, member)
            .Map(validatedValue => new TransferId(validatedValue));
    public static Result<TransferId> Create(Guid value, string propertyName = nameof(TransferId))
        => Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new TransferId(validatedValue));
    public static Result<TransferId> Create(Guid value, PropertyPath path, string member)
        => Result.NotEmpty(value, path, member)
            .Map(validatedValue => new TransferId(validatedValue));

    private TransferId(Guid value) : base(value) { }
}

[Alias("Tests.OpeningBalance"), GenerateSerializer, Immutable]
public sealed record OpeningBalance : DecimalValueObject<OpeningBalance>
{
    public const decimal MinimumValue = 0m;
    public const decimal MaximumValue = 1_000_000m;

    public static Result<OpeningBalance> Create(decimal value, string propertyName = nameof(OpeningBalance))
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, propertyName)
            .Map(validatedValue => new OpeningBalance(validatedValue));
    public static Result<OpeningBalance> Create(decimal value, PropertyPath path, string member)
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, path, member)
            .Map(validatedValue => new OpeningBalance(validatedValue));

    private OpeningBalance(decimal value) : base(value) { }
}

[Alias("Tests.OverDraftLimit"), GenerateSerializer, Immutable]
public sealed record OverDraftLimit : DecimalValueObject<OverDraftLimit>
{
    public const decimal MinimumValue = 0m;
    public const decimal MaximumValue = 1_000m;

    public static Result<OverDraftLimit> Create(decimal value, string propertyName = nameof(OverDraftLimit))
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, propertyName)
            .Map(validatedValue => new OverDraftLimit(validatedValue));
    public static Result<OverDraftLimit> Create(decimal value, PropertyPath path, string member)
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, path, member)
            .Map(validatedValue => new OverDraftLimit(validatedValue));

    private OverDraftLimit(decimal value) : base(value) { }
}

[Alias("Tests.CreditAmount"), GenerateSerializer, Immutable]
public sealed record CreditAmount : DecimalValueObject<CreditAmount>
{
    public const decimal MinimumValue = 0.01m;
    public const decimal MaximumValue = 1_000_000m;

    public static Result<CreditAmount> Create(decimal value, string propertyName = nameof(CreditAmount))
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, propertyName)
            .Map(validatedValue => new CreditAmount(validatedValue));
    public static Result<CreditAmount> Create(decimal value, PropertyPath path, string member)
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, path, member)
            .Map(validatedValue => new CreditAmount(validatedValue));

    private CreditAmount(decimal value) : base(value) { }
}

[Alias("Tests.TransferAmount"), GenerateSerializer, Immutable]
public sealed record TransferAmount : DecimalValueObject<TransferAmount>
{
    public const decimal MinimumValue = 0.01m;
    public const decimal MaximumValue = 1_000_000m;

    public static Result<TransferAmount> Create(decimal value, string propertyName = nameof(TransferAmount))
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, propertyName)
            .Map(validatedValue => new TransferAmount(validatedValue));
    public static Result<TransferAmount> Create(decimal value, PropertyPath path, string member)
        => Result.InclusiveBetween(value, MinimumValue, MaximumValue, path, member)
            .Map(validatedValue => new TransferAmount(validatedValue));

    private TransferAmount(decimal value) : base(value) { }
}