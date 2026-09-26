#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is a valid Guid and not equal to Guid.Empty.
    /// </summary>
    public static Result<Guid> IsGuidAndNotEmpty(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) 
            return Result.Failure<Guid>(result.Error);

        if (!Guid.TryParse(result.Value, out Guid guidValue)) 
            return Result.Failure<Guid>(new ValidationError(propertyName, RegularExpressionError, propertyName));

        return guidValue != Guid.Empty
            ? Result.Success(guidValue)
            : Result.Failure<Guid>(new ValidationError(propertyName, NotEmptyError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid Guid.
    /// </summary>
    public static Result<Guid> IsGuidAndNotEmpty(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) 
            return Result.Failure<Guid>(result.Error);

        var isNotAGuid = !Guid.TryParse(result.Value, out Guid guidValue);

        if (isNotAGuid || guidValue == Guid.Empty)
        {
            var errorFormat = isNotAGuid ? RegularExpressionError : NotEmptyError;
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, errorFormat, propertyName));
        }

        return Result.Success(guidValue);
    }
}
