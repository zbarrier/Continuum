#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string ScalePrecisionError = "'{0}' must not be more than {1} digits in total, with allowance for {2} decimals. {3} digits and {4} decimals were found.";

    /// <summary>
    ///     Ensure value has a certain scale and precision.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="expectedScale">The expected number of digits after the decimal point.</param>
    /// <param name="expectedPrecision">The expected total number of digits.</param>
    /// <param name="ignoreTrailingZeroes">Ignore any trailing zeroes or not.</param>
    /// <param name="propertyName">The name of the property being checked.</param>
    /// <returns>A success or failure result.</returns>
    public static Result<decimal> ScalePrecision(decimal value, int expectedScale, int expectedPrecision, bool ignoreTrailingZeroes, string propertyName)
    {
        ScalePrecisionHelpers.ValidateAndCalculateScaleAndPrecision(value, expectedScale, expectedPrecision, ignoreTrailingZeroes, 
            out int actualScale, out int actualPrecision, 
            out int actualIntegerDigits, out int expectedIntegerDigits);

        if (actualScale > expectedScale || actualIntegerDigits > expectedIntegerDigits)
        {
            var digits = actualIntegerDigits < 0 ? 0 : actualIntegerDigits;
            return Result.Failure<decimal>(new ValidationError(propertyName, ScalePrecisionError, propertyName, expectedPrecision, expectedScale, digits, actualScale));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value has a certain scale and precision.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="expectedScale">The expected number of digits after the decimal point.</param>
    /// <param name="expectedPrecision">The expected total number of digits.</param>
    /// <param name="ignoreTrailingZeroes">Ignore any trailing zeroes or not.</param>
    /// <param name="propertyName">The name of the property being checked.</param>
    /// <returns>A success or failure result.</returns>
    public static Result<decimal> ScalePrecision(decimal value, int expectedScale, int expectedPrecision, bool ignoreTrailingZeroes, string propertyNameFormat, params object[] arguments)
    {
        ScalePrecisionHelpers.ValidateAndCalculateScaleAndPrecision(value, expectedScale, expectedPrecision, ignoreTrailingZeroes,
            out int actualScale, out int actualPrecision,
            out int actualIntegerDigits, out int expectedIntegerDigits);

        if (actualScale > expectedScale || actualIntegerDigits > expectedIntegerDigits)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            var digits = actualIntegerDigits < 0 ? 0 : actualIntegerDigits;
            return Result.Failure<decimal>(new ValidationError(propertyName, ScalePrecisionError, propertyName, expectedPrecision, expectedScale, digits, actualScale));
        }

        return Result.Success(value);
    }
}
