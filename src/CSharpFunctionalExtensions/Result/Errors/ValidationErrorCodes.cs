namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Machine-readable codes assigned to <see cref="ValidationErrorEntry.Code"/> by the built-in validators.
///     These codes are also the keys used to look up localized message templates.
/// </summary>
public static class ValidationErrorCodes
{
    /// <summary>The collection does not contain a number of items within the required range.</summary>
    public const string CountBetween = "count_between";
    /// <summary>The value is not a valid credit card number.</summary>
    public const string CreditCard = "credit_card";
    /// <summary>The value is not a valid email address.</summary>
    public const string Email = "email";
    /// <summary>The value is not equal to the expected value.</summary>
    public const string Equal = "equal";
    /// <summary>The collection does not contain exactly the required number of items.</summary>
    public const string ExactCount = "exact_count";
    /// <summary>The value does not have exactly the required length.</summary>
    public const string ExactLength = "exact_length";
    /// <summary>The value is not within the required range (exclusive).</summary>
    public const string ExclusiveBetween = "exclusive_between";
    /// <summary>The value is not greater than the comparison value.</summary>
    public const string GreaterThan = "greater_than";
    /// <summary>The value is not greater than or equal to the comparison value.</summary>
    public const string GreaterThanOrEqual = "greater_than_or_equal";
    /// <summary>The value is not within the required range (inclusive).</summary>
    public const string InclusiveBetween = "inclusive_between";
    /// <summary>The value does not match the required format or regular expression.</summary>
    public const string InvalidFormat = "invalid_format";
    /// <summary>The value does not have a length within the required range.</summary>
    public const string LengthBetween = "length_between";
    /// <summary>The value is not less than the comparison value.</summary>
    public const string LessThan = "less_than";
    /// <summary>The value is not less than or equal to the comparison value.</summary>
    public const string LessThanOrEqual = "less_than_or_equal";
    /// <summary>The collection contains more than the maximum number of items.</summary>
    public const string MaxCount = "max_count";
    /// <summary>The value is longer than the maximum length.</summary>
    public const string MaxLength = "max_length";
    /// <summary>The collection contains fewer than the minimum number of items.</summary>
    public const string MinCount = "min_count";
    /// <summary>The value is shorter than the minimum length.</summary>
    public const string MinLength = "min_length";
    /// <summary>The value is null, empty, or the default value.</summary>
    public const string NotEmpty = "not_empty";
    /// <summary>The value is null.</summary>
    public const string NotNull = "not_null";
    /// <summary>The value exceeds the allowed scale or precision.</summary>
    public const string ScalePrecision = "scale_precision";
}
