#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<T> GreaterThanOrEqual<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyName)
    {
        return comparer.Compare(value, comparisonValue) >= 0
            ? Result.Success(value)
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, ErrorArgument.FromObject(comparisonValue)));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<T> GreaterThanOrEqual<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (comparer.Compare(value, comparisonValue) < 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, ErrorArgument.FromObject(comparisonValue)));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<string> GreaterThanOrEqual(string value, string comparisonValue, string propertyName)
        => value.CompareTo(comparisonValue) >= 0
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<string> GreaterThanOrEqual(string value, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value.CompareTo(comparisonValue) < 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<byte> GreaterThanOrEqual(byte value, byte comparisonValue, string propertyName) 
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<byte> GreaterThanOrEqual(byte value, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<short> GreaterThanOrEqual(short value, short comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<short> GreaterThanOrEqual(short value, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<int> GreaterThanOrEqual(int value, int comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<int> GreaterThanOrEqual(int value, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<long> GreaterThanOrEqual(long value, long comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<long> GreaterThanOrEqual(long value, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<float> GreaterThanOrEqual(float value, float comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<float> GreaterThanOrEqual(float value, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<double> GreaterThanOrEqual(double value, double comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<double> GreaterThanOrEqual(double value, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<decimal> GreaterThanOrEqual(decimal value, decimal comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<decimal> GreaterThanOrEqual(decimal value, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<BigInteger> GreaterThanOrEqual(BigInteger value, BigInteger comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<BigInteger> GreaterThanOrEqual(BigInteger value, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThanOrEqual(TimeSpan value, TimeSpan comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThanOrEqual(TimeSpan value, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateOnly> GreaterThanOrEqual(DateOnly value, DateOnly comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateOnly> GreaterThanOrEqual(DateOnly value, DateOnly comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }


    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTime> GreaterThanOrEqual(DateTime value, DateTime comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTime> GreaterThanOrEqual(DateTime value, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThanOrEqual(DateTimeOffset value, DateTimeOffset comparisonValue, string propertyName)
        => value >= comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThanOrEqual(DateTimeOffset value, DateTimeOffset comparisonValue, 
        string propertyNameFormat, params object[] arguments)
    {
        if (value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }
}
