#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrWhitespace(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        if (string.IsNullOrWhiteSpace(result.Value))
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrWhitespace(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (string.IsNullOrWhiteSpace(result.Value))
        {
            string propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }
            
        return Result.Success(result.Value);
    }
}
