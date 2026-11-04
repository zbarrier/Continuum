#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is not null, empty or consists only of whitespace.
    /// </summary>
    public static Result<string> NotNullOrWhitespace(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or consists only of whitespace.
    /// </summary>
    public static Result<string> NotNullOrWhitespace(string? value, PropertyPath path, string member)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }
            
        return Result.Success(value);
    }
}
