#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    public static Result<decimal> IsDecimal(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        return decimal.TryParse(result.Value, out decimal decimalValue)
            ? Result.Success(decimalValue)
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    public static Result<decimal> IsDecimal(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        if (!decimal.TryParse(result.Value, out decimal decimalValue))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(decimalValue);
    }
}
