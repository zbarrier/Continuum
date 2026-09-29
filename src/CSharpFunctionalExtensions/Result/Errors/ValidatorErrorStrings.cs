namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Default (English) message templates for validators. Placeholders are positional:
///     {0} is always the property name. Length/count templates receive the actual length/count
///     as their final argument ({2} for min/max/exact, {3} for between) so custom or localized
///     templates may use it.
/// </summary>
public static class ValidatorErrorStrings
{
    /// <summary>{0} property, {1} min, {2} max, {3} actual count.</summary>
    public const string CountBetween = "'{0}' must contain between {1} and {2} items.";
    /// <summary>{0} property, {1} expected count, {2} actual count.</summary>
    public const string ExactCount = "'{0}' must contain exactly {1} items.";
    /// <summary>{0} property, {1} min length, {2} max length, {3} actual length.</summary>
    public const string LengthBetween = "'{0}' must be between {1} and {2} characters.";
    /// <summary>{0} property, {1} max count, {2} actual count.</summary>
    public const string MaxCount = "'{0}' must contain {1} items or fewer.";
    /// <summary>{0} property, {1} min count, {2} actual count.</summary>
    public const string MinCount = "'{0}' must contain at least {1} items.";

    /// <summary>{0} property.</summary>
    public const string CreditCard = "'{0}' is not a valid credit card number.";
    /// <summary>{0} property.</summary>
    public const string Email = "'{0}' is not a valid email address.";
    /// <summary>{0} property, {1} expected value.</summary>
    public const string Equal = "'{0}' must be equal to '{1}'.";
    /// <summary>{0} property, {1} expected length, {2} actual length.</summary>
    public const string ExactLength = "'{0}' must be {1} characters in length.";
    /// <summary>{0} property, {1} lower bound, {2} upper bound, {3} actual value.</summary>
    public const string ExclusiveBetween = "'{0}' must be between {1} and {2} (exclusive). You entered {3}.";
    /// <summary>{0} property, {1} comparison value.</summary>
    public const string GreaterThan = "'{0}' must be greater than '{1}'.";
    /// <summary>{0} property, {1} comparison value.</summary>
    public const string GreaterThanOrEqual = "'{0}' must be greater than or equal to '{1}'.";
    /// <summary>{0} property, {1} lower bound, {2} upper bound, {3} actual value.</summary>
    public const string InclusiveBetween = "'{0}' must be between {1} and {2}. You entered {3}.";
    /// <summary>{0} property, {1} comparison value.</summary>
    public const string LessThan = "'{0}' must be less than '{1}'.";
    /// <summary>{0} property, {1} comparison value.</summary>
    public const string LessThanOrEqual = "'{0}' must be less than or equal to '{1}'.";
    /// <summary>{0} property.</summary>
    public const string InvalidFormat = "'{0}' is not in the correct format.";
    /// <summary>{0} property, {1} max length, {2} actual length.</summary>
    public const string MaxLength = "The length of '{0}' must be {1} characters or fewer.";
    /// <summary>{0} property, {1} min length, {2} actual length.</summary>
    public const string MinLength = "The length of '{0}' must be at least {1} characters.";
    /// <summary>{0} property.</summary>
    public const string NotEmpty = "'{0}' must not be empty.";
    /// <summary>{0} property.</summary>
    public const string NotNull = "'{0}' must not be empty.";
    /// <summary>{0} property, {1} expected precision, {2} expected scale, {3} actual integer digits, {4} actual scale.</summary>
    public const string ScalePrecision = "'{0}' must not be more than {1} digits in total, with allowance for {2} decimals. {3} digits and {4} decimals were found.";
}
