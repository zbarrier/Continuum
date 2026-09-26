#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        return DateOnly.TryParse(result.Value, out DateOnly dateValue)
            ? Result.Success(dateValue)
            : Result.Failure<DateOnly>(new ValidationError(propertyName, RegularExpressionError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        if (!DateOnly.TryParse(result.Value, out DateOnly dateValue))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, RegularExpressionError, propertyName));
        }

        return Result.Success(dateValue);
    }
}
