#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    private const char HyphenChar = '-';
    private const string HyphenStr = "-";

    const string ZipCodeSimpleError = "'{0}' is not a valid US zip code.";

    const int NonHyphenLength = 5;
    const int HyphenLength = 10;

    const int HyphenIndex = 5;

    /// <summary>
    ///     Ensure value is valid US zip code.
    /// </summary>
    public static Result<string> IsUSZipCode(string value, string propertyName)
    {
        // Check length
        bool isValidLength = value.Length == NonHyphenLength || value.Length == HyphenLength;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ZipCodeSimpleError, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, ZipCodeSimpleError, propertyName));
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
            return Result.Failure<string>(new ValidationError(propertyName, ZipCodeSimpleError, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, ZipCodeSimpleError, propertyName));
            }
        }

        return Result.Success(value);
    }
}
