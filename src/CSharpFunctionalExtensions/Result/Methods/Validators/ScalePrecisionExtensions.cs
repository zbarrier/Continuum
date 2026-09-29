#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value has a certain scale and precision.
    /// </summary>
    /// <param name="result">The result whose value to check.</param>
    /// <param name="expectedScale">The expected number of digits after the decimal point.</param>
    /// <param name="expectedPrecision">The expected total number of digits.</param>
    /// <param name="ignoreTrailingZeroes">Ignore any trailing zeroes or not.</param>
    /// <param name="propertyName">The name of the property being checked.</param>
    /// <returns>A success or failure result.</returns>
    public static Result<decimal> ScalePrecision(this Result<decimal> result, int expectedScale, int expectedPrecision, bool ignoreTrailingZeroes, string propertyName)
    {
        if (result.IsFailure) return result;

        ScalePrecisionHelpers.ValidateAndCalculateScaleAndPrecision(result.Value, expectedScale, expectedPrecision, ignoreTrailingZeroes, 
            out int actualScale, out int actualPrecision, 
            out int actualIntegerDigits, out int expectedIntegerDigits);

        if (actualScale > expectedScale || actualIntegerDigits > expectedIntegerDigits)
        {
            var digits = actualIntegerDigits < 0 ? 0 : actualIntegerDigits;
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.ScalePrecision, ValidatorErrorStrings.ScalePrecision, propertyName, expectedPrecision, expectedScale, digits, actualScale));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value has a certain scale and precision.
    /// </summary>
    /// <param name="result">The result whose value to check.</param>
    /// <param name="expectedScale">The expected number of digits after the decimal point.</param>
    /// <param name="expectedPrecision">The expected total number of digits.</param>
    /// <param name="ignoreTrailingZeroes">Ignore any trailing zeroes or not.</param>
    /// <param name="propertyNameFormat">A composite format string for the name of the property being checked.</param>
    /// <param name="arguments">The arguments used to format <paramref name="propertyNameFormat"/>.</param>
    /// <returns>A success or failure result.</returns>
    public static Result<decimal> ScalePrecision(this Result<decimal> result, int expectedScale, int expectedPrecision, bool ignoreTrailingZeroes, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        ScalePrecisionHelpers.ValidateAndCalculateScaleAndPrecision(result.Value, expectedScale, expectedPrecision, ignoreTrailingZeroes,
            out int actualScale, out int actualPrecision,
            out int actualIntegerDigits, out int expectedIntegerDigits);

        if (actualScale > expectedScale || actualIntegerDigits > expectedIntegerDigits)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            var digits = actualIntegerDigits < 0 ? 0 : actualIntegerDigits;
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.ScalePrecision, ValidatorErrorStrings.ScalePrecision, propertyName, expectedPrecision, expectedScale, digits, actualScale));
        }

        return result;
    }
}
