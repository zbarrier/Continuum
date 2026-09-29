#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     US postal state abbreviation validators exposed as static members of <see cref="Result"/>.
/// </summary>
public static partial class USStateAbbreviationValidators
{
    private static HashSet<string> USPostalStateAbbreviations = new(StringComparer.OrdinalIgnoreCase)

    {

        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",

        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",

        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",

        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",

        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",

        // Including territories

        "DC", // District of Columbia

        "AS", // American Samoa

        "GU", // Guam

        "MP", // Northern Mariana Islands

        "PR", // Puerto Rico

        "VI"  // U.S. Virgin Islands

    };

    extension(Result)
    {
        /// <summary>
        ///     Ensure value is valid US postal state abbreviation.
        /// </summary>
        public static Result<string> IsUSPostalStateAbbreviation(string value, string propertyName)
        {
            return USPostalStateAbbreviations.Contains(value)
                ? Result.Success(value)
                : Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.StateAbbreviation, USValidatorErrorStrings.StateAbbreviation, propertyName));
        }

        /// <summary>
        ///     Ensure value is valid US postal state abbreviation.
        /// </summary>
        public static Result<string> IsUSPostalStateAbbreviation(string value, string propertyNameFormat, params object[] arguments)
        {
            if (!USPostalStateAbbreviations.Contains(value))
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.StateAbbreviation, USValidatorErrorStrings.StateAbbreviation, propertyName));
            }
            return Result.Success(value);
        }
    }
}
