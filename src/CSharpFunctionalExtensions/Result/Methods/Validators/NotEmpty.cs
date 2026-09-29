#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotEmpty<T>(IEnumerable<T> value, string propertyName)
        => value is null || !value.Any()
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotEmpty<T>(IEnumerable<T> value, string propertyNameFormat, params object[] arguments)
    {
        if (value is null || !value.Any())
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<string> NotEmpty(string? value, string propertyName) 
        => string.IsNullOrEmpty(value)
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<string> NotEmpty(string? value, string propertyNameFormat, params object[] arguments)
    {
        if (string.IsNullOrEmpty(value))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<Guid> NotEmpty(Guid value, string propertyName) 
        => value == Guid.Empty
            ? Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<Guid> NotEmpty(Guid value, string propertyNameFormat, params object[] arguments)
    {
        if (value == Guid.Empty)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<byte> NotEmpty(byte value, string propertyName) 
        => value == default
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<byte> NotEmpty(byte value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<short> NotEmpty(short value, string propertyName) 
        => value == default
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<short> NotEmpty(short value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<int> NotEmpty(int value, string propertyName) 
        => value == default
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<int> NotEmpty(int value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<long> NotEmpty(long value, string propertyName) 
        => value == default
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<long> NotEmpty(long value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<float> NotEmpty(float value, string propertyName) 
        => value == default
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<float> NotEmpty(float value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<double> NotEmpty(double value, string propertyName) 
        => value == default
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<double> NotEmpty(double value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<decimal> NotEmpty(decimal value, string propertyName) 
        => value == default
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<decimal> NotEmpty(decimal value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<BigInteger> NotEmpty(BigInteger value, string propertyName)
        => value == BigInteger.Zero
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<BigInteger> NotEmpty(BigInteger value, string propertyNameFormat, params object[] arguments)
    {
        if (value == BigInteger.Zero)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeSpan> NotEmpty(TimeSpan value, string propertyName)
        => value == TimeSpan.Zero
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeSpan> NotEmpty(TimeSpan value, string propertyNameFormat, params object[] arguments)
    {
        if (value == TimeSpan.Zero)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTime> NotEmpty(DateTime value, string propertyName)
        => value == default(DateTime)
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTime> NotEmpty(DateTime value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default(DateTime))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotEmpty(DateTimeOffset value, string propertyName)
        => value == default(DateTimeOffset)
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotEmpty(DateTimeOffset value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default(DateTimeOffset))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateOnly> NotEmpty(DateOnly value, string propertyName)
        => value == default(DateOnly)
            ? Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateOnly> NotEmpty(DateOnly value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default(DateOnly))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeOnly> NotEmpty(TimeOnly value, string propertyName)
        => value == default(TimeOnly)
            ? Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeOnly> NotEmpty(TimeOnly value, string propertyNameFormat, params object[] arguments)
    {
        if (value == default(TimeOnly))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }
}
