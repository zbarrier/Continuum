#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure collection count is between min and max (inclusive).
    /// </summary>
    public static Result<IEnumerable<T>> CountBetween<T>(this Result<IEnumerable<T>> result, int min, int max, string propertyName)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count < min || count > max)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.CountBetween, ValidatorErrorStrings.CountBetween, propertyName, min, max, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure collection count is between min and max (inclusive).
    /// </summary>
    public static Result<IEnumerable<T>> CountBetween<T>(this Result<IEnumerable<T>> result, int min, int max, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count < min || count > max)
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.CountBetween, ValidatorErrorStrings.CountBetween, propertyName, min, max, count));
        }

        return result;
    }
}
