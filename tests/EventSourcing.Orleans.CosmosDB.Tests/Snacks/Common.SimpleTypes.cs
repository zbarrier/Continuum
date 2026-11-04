using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Snacks;

[Alias("Tests.SnackId"), GenerateSerializer, Immutable]
public sealed record SnackId : GuidValueObject<SnackId>
{
    public static Result<SnackId> Create(string? value, string propertyName = nameof(SnackId))
        => Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new SnackId(validatedValue));
    public static Result<SnackId> Create(string? value, PropertyPath path, string member)
        => Result.IsGuidAndNotEmpty(value, path, member)
            .Map(validatedValue => new SnackId(validatedValue));
    public static Result<SnackId> Create(Guid value, string propertyName = nameof(SnackId))
        => Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new SnackId(validatedValue));
    public static Result<SnackId> Create(Guid value, PropertyPath path, string member)
        => Result.NotEmpty(value, path, member)
            .Map(validatedValue => new SnackId(validatedValue));

    private SnackId(Guid value) : base(value) { }
}

[Alias("Tests.SnackName"), GenerateSerializer, Immutable]
public sealed record SnackName : StringValueObject<SnackName>
{
    public const int MaximumLength = 100;

    public static Result<SnackName> Create(string? value, string propertyName = nameof(SnackName))
        => Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaximumLength, propertyName)
            .Map(validatedValue => new SnackName(validatedValue));
    public static Result<SnackName> Create(string? value, PropertyPath path, string member)
        => Result.NotNullOrWhitespace(value, path, member)
            .MaxLength(MaximumLength, path, member)
            .Map(validatedValue => new SnackName(validatedValue));

    private SnackName(string value) : base(value) { }
}
