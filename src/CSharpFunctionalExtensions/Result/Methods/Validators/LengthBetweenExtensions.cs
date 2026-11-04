#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure string length is between min and max (inclusive).
    /// </summary>
    public static Result<string> LengthBetween(this Result<string> result, int min, int max, string propertyName)
    {
        if (result.IsFailure) return result;

        var length = result.Value.Length;
        if (length < min || length > max)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LengthBetween, ValidatorErrorStrings.LengthBetween, propertyName, min, max, length));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is between min and max (inclusive).
    /// </summary>
    public static Result<string> LengthBetween(this Result<string> result, int min, int max, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        var length = result.Value.Length;
        if (length < min || length > max)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LengthBetween, ValidatorErrorStrings.LengthBetween, propertyName, min, max, length));
        }

        return result;
    }
}
