#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<T> Equal<T>(T value, T expectedValue, IEqualityComparer<T> comparer, string propertyName)
    {
        bool success = comparer is not null
            ? comparer.Equals(value, expectedValue)
            : Equals(value, expectedValue);

        return success 
            ? Result.Success(value)
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, ErrorArgument.FromObject(expectedValue)));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<T> Equal<T>(T value, T expectedValue, IEqualityComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        bool success = comparer is not null
            ? comparer.Equals(value, expectedValue)
            : Equals(value, expectedValue);

        if (!success)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, ErrorArgument.FromObject(expectedValue)));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<string> Equal(string value, string expectedValue, StringComparison stringComparisonType, string propertyName)
        => value.Equals(expectedValue, stringComparisonType)
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<string> Equal(string value, string expectedValue, StringComparison stringComparisonType,
        string propertyNameFormat, params object[] arguments)
    {
        if (!value.Equals(expectedValue, stringComparisonType))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<byte> Equal(byte value, byte expectedValue, string propertyName) 
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<byte> Equal(byte value, byte expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<short> Equal(short value, short expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<short> Equal(short value, short expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<int> Equal(int value, int expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<int> Equal(int value, int expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<long> Equal(long value, long expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<long> Equal(long value, long expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<float> Equal(float value, float expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<float> Equal(float value, float expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<double> Equal(double value, double expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<double> Equal(double value, double expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<decimal> Equal(decimal value, decimal expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<decimal> Equal(decimal value, decimal expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<BigInteger> Equal(BigInteger value, BigInteger expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<BigInteger> Equal(BigInteger value, BigInteger expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<TimeSpan> Equal(TimeSpan value, TimeSpan expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<TimeSpan> Equal(TimeSpan value, TimeSpan expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTime> Equal(DateTime value, DateTime expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTime> Equal(DateTime value, DateTime expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> Equal(DateTimeOffset value, DateTimeOffset expectedValue, string propertyName)
        => value == expectedValue
            ? Result.Success(value)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> Equal(DateTimeOffset value, DateTimeOffset expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, propertyName, expectedValue));
        }

        return Result.Success(value);
    }
}
