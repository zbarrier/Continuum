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
            return Result.Failure<string>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or consists only of whitespace.
    /// </summary>
    public static Result<string> NotNullOrWhitespace(string? value, string propertyNameFormat, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }
            
        return Result.Success(value);
    }
}
