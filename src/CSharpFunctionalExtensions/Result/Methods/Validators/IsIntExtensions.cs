#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    public static Result<int> IsInt32(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        return int.TryParse(result.Value, out int intValue)
            ? Result.Success(intValue)
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    public static Result<int> IsInt32(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        if (!int.TryParse(result.Value, out int intValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(intValue);
    }
}
