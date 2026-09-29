#nullable enable

using System.Collections.Concurrent;

namespace Continuum.CSharpFunctionalExtensions;

internal static class EnglishLanguage
{
    public const string Culture = "en";

    private static readonly ConcurrentDictionary<string, string> _registered = new(StringComparer.Ordinal);

    public static string? GetTranslation(string code) =>
        GetBuiltInTranslation(code) ?? (_registered.TryGetValue(code, out var template) ? template : null);

    public static void Register(string code, string template)
    {
        if (GetBuiltInTranslation(code) is not null)
        {
            throw new ArgumentException($"'{code}' is a built-in error code and cannot be registered.", nameof(code));
        }

        var existing = _registered.GetOrAdd(code, template);
        if (!string.Equals(existing, template, StringComparison.Ordinal))
        {
            throw new ArgumentException($"'{code}' is already registered with a different template.", nameof(code));
        }
    }

    private static string? GetBuiltInTranslation(string code) => code switch
    {
        ValidationErrorCodes.CountBetween => ValidatorErrorStrings.CountBetween,
        ValidationErrorCodes.CreditCard => ValidatorErrorStrings.CreditCard,
        ValidationErrorCodes.Email => ValidatorErrorStrings.Email,
        ValidationErrorCodes.Equal => ValidatorErrorStrings.Equal,
        ValidationErrorCodes.ExactCount => ValidatorErrorStrings.ExactCount,
        ValidationErrorCodes.ExactLength => ValidatorErrorStrings.ExactLength,
        ValidationErrorCodes.ExclusiveBetween => ValidatorErrorStrings.ExclusiveBetween,
        ValidationErrorCodes.GreaterThan => ValidatorErrorStrings.GreaterThan,
        ValidationErrorCodes.GreaterThanOrEqual => ValidatorErrorStrings.GreaterThanOrEqual,
        ValidationErrorCodes.InclusiveBetween => ValidatorErrorStrings.InclusiveBetween,
        ValidationErrorCodes.InvalidFormat => ValidatorErrorStrings.InvalidFormat,
        ValidationErrorCodes.LengthBetween => ValidatorErrorStrings.LengthBetween,
        ValidationErrorCodes.LessThan => ValidatorErrorStrings.LessThan,
        ValidationErrorCodes.LessThanOrEqual => ValidatorErrorStrings.LessThanOrEqual,
        ValidationErrorCodes.MaxCount => ValidatorErrorStrings.MaxCount,
        ValidationErrorCodes.MaxLength => ValidatorErrorStrings.MaxLength,
        ValidationErrorCodes.MinCount => ValidatorErrorStrings.MinCount,
        ValidationErrorCodes.MinLength => ValidatorErrorStrings.MinLength,
        ValidationErrorCodes.NotEmpty => ValidatorErrorStrings.NotEmpty,
        ValidationErrorCodes.NotNull => ValidatorErrorStrings.NotNull,
        ValidationErrorCodes.ScalePrecision => ValidatorErrorStrings.ScalePrecision,

        ErrorCodes.Aborted => RequestErrorStrings.Aborted,
        ErrorCodes.AlreadyExists => RequestErrorStrings.AlreadyExists,
        ErrorCodes.Cancelled => RequestErrorStrings.Cancelled,
        ErrorCodes.DataLoss => RequestErrorStrings.DataLoss,
        ErrorCodes.DeadlineExceeded => RequestErrorStrings.DeadlineExceeded,
        ErrorCodes.FailedPrecondition => RequestErrorStrings.FailedPrecondition,
        ErrorCodes.Internal => RequestErrorStrings.Internal,
        ErrorCodes.InvalidArgument => RequestErrorStrings.InvalidArgument,
        ErrorCodes.NotFound => RequestErrorStrings.NotFound,
        ErrorCodes.OutOfRange => RequestErrorStrings.OutOfRange,
        ErrorCodes.PermissionDenied => RequestErrorStrings.PermissionDenied,
        ErrorCodes.ResourceExhausted => RequestErrorStrings.ResourceExhausted,
        ErrorCodes.Unauthenticated => RequestErrorStrings.Unauthenticated,
        ErrorCodes.Unavailable => RequestErrorStrings.Unavailable,
        ErrorCodes.Unimplemented => RequestErrorStrings.Unimplemented,
        ErrorCodes.Unknown => RequestErrorStrings.Unknown,
        ErrorCodes.ValidationFailed => RequestErrorStrings.ValidationFailed,
        _ => null,
    };
}
