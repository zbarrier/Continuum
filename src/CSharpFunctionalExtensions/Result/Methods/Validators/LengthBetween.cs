#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure string length is between min and max (inclusive).
    /// </summary>
    public static Result<string> LengthBetween(string value, int min, int max, string propertyName)
    {
        var length = value.Length;
        if (length < min || length > max)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LengthBetween, ValidatorErrorStrings.LengthBetween, propertyName, min, max, length));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is between min and max (inclusive).
    /// </summary>
    public static Result<string> LengthBetween(string value, int min, int max, PropertyPath path, string member)
    {
        var length = value.Length;
        if (length < min || length > max)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LengthBetween, ValidatorErrorStrings.LengthBetween, propertyName, min, max, length));
        }

        return Result.Success(value);
    }
}
