namespace Continuum.CSharpFunctionalExtensions.Tests;

// Intentionally duplicated from ValidationErrorCodes so that any change to a
// library code is caught by the tests instead of silently flowing through.
public static class ExpectedValidationErrorCodes
{
    public const string CountBetween = "count_between";
    public const string CreditCard = "credit_card";
    public const string Email = "email";
    public const string Equal = "equal";
    public const string ExactCount = "exact_count";
    public const string ExactLength = "exact_length";
    public const string ExclusiveBetween = "exclusive_between";
    public const string GreaterThan = "greater_than";
    public const string GreaterThanOrEqual = "greater_than_or_equal";
    public const string InclusiveBetween = "inclusive_between";
    public const string InvalidFormat = "invalid_format";
    public const string LengthBetween = "length_between";
    public const string LessThan = "less_than";
    public const string LessThanOrEqual = "less_than_or_equal";
    public const string MaxCount = "max_count";
    public const string MaxLength = "max_length";
    public const string MinCount = "min_count";
    public const string MinLength = "min_length";
    public const string NotEmpty = "not_empty";
    public const string NotNull = "not_null";
    public const string ScalePrecision = "scale_precision";
}
