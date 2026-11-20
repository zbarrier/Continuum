#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    public static Result<long> IsInt64(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        return long.TryParse(result.Value, out long longValue)
            ? Result.Success(longValue)
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    public static Result<long> IsInt64(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        if (!long.TryParse(result.Value, out long longValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(longValue);
    }
}
