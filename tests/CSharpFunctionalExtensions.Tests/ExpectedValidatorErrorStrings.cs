namespace Continuum.CSharpFunctionalExtensions.Tests;

// Intentionally duplicated from ValidatorErrorStrings so that any change to a
// library message is caught by the tests instead of silently flowing through.
public static class ExpectedValidatorErrorStrings
{
    public const string CountBetween = "'{0}' must contain between {1} and {2} items.";
    public const string ExactCount = "'{0}' must contain exactly {1} items.";
    public const string LengthBetween = "'{0}' must be between {1} and {2} characters.";
    public const string MaxCount = "'{0}' must contain {1} items or fewer.";
    public const string MinCount = "'{0}' must contain at least {1} items.";
    public const string CreditCard = "'{0}' is not a valid credit card number.";
    public const string Email = "'{0}' is not a valid email address.";
    public const string Equal = "'{0}' must be equal to '{1}'.";
    public const string ExactLength = "'{0}' must be {1} characters in length.";
    public const string ExclusiveBetween = "'{0}' must be between {1} and {2} (exclusive). You entered {3}.";
    public const string GreaterThan = "'{0}' must be greater than '{1}'.";
    public const string GreaterThanOrEqual = "'{0}' must be greater than or equal to '{1}'.";
    public const string InclusiveBetween = "'{0}' must be between {1} and {2}. You entered {3}.";
    public const string LessThan = "'{0}' must be less than '{1}'.";
    public const string LessThanOrEqual = "'{0}' must be less than or equal to '{1}'.";
    public const string InvalidFormat = "'{0}' is not in the correct format.";
    public const string MaxLength = "The length of '{0}' must be {1} characters or fewer.";
    public const string MinLength = "The length of '{0}' must be at least {1} characters.";
    public const string NotEmpty = "'{0}' must not be empty.";
    public const string NotNull = "'{0}' must not be empty.";
    public const string ScalePrecision = "'{0}' must not be more than {1} digits in total, with allowance for {2} decimals. {3} digits and {4} decimals were found.";
}
