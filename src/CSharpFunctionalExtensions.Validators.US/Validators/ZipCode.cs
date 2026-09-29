#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     US zip code validators exposed as static members of <see cref="Result"/>.
/// </summary>
public static partial class USZipCodeValidators
{
    private const char HyphenChar = '-';

    private const string HyphenStr = "-";

    const int NonHyphenLength = 5;

    const int HyphenLength = 10;

    const int HyphenIndex = 5;

    extension(Result)
    {
        /// <summary>
        ///     Ensure value is valid US zip code.
        /// </summary>
        public static Result<string> IsUSZipCode(string value, string propertyName)
        {
            // Check length
            bool isValidLength = value.Length == NonHyphenLength || value.Length == HyphenLength;
            if (!isValidLength)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ZipCode, USValidatorErrorStrings.ZipCode, propertyName));
            }

            // Check characters
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                bool isValidChar = i == HyphenIndex
                    ? c == '-'
                    : char.IsDigit(c);

                if (!isValidChar)
                {
                    return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ZipCode, USValidatorErrorStrings.ZipCode, propertyName));
                }
            }

            return Result.Success(value);
        }

        /// <summary>
        ///     Ensure value is valid US zip code.
        /// </summary>
        public static Result<string> IsUSZipCode(string value, string propertyNameFormat, params object[] arguments)
        {
            // Check length
            bool isValidLength = value.Length == NonHyphenLength || value.Length == HyphenLength;
            if (!isValidLength)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ZipCode, USValidatorErrorStrings.ZipCode, propertyName));
            }

            // Check characters
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                bool isValidChar = i == HyphenIndex
                    ? c == '-'
                    : char.IsDigit(c);

                if (!isValidChar)
                {
                    var propertyName = string.Format(propertyNameFormat, arguments);
                    return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ZipCode, USValidatorErrorStrings.ZipCode, propertyName));
                }
            }

            return Result.Success(value);
        }
    }
}
