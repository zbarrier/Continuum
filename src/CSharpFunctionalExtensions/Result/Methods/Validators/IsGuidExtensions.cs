#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid Guid.
    /// </summary>
    public static Result<Guid> IsGuid(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        return Guid.TryParse(result.Value, out Guid guidValue)
            ? Result.Success(guidValue)
            : Result.Failure<Guid>(new ValidationError(propertyName, RegularExpressionError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid Guid.
    /// </summary>
    public static Result<Guid> IsGuid(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        if (!Guid.TryParse(result.Value, out Guid guidValue))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, RegularExpressionError, propertyName));
        }

        return Result.Success(guidValue);
    }
}
