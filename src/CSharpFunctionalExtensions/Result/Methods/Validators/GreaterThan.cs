#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string GreaterThanError = "'{0}' must be greater than '{1}'.";

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<T> GreaterThan<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyName) 
        => comparer.Compare(value, comparisonValue) > 0
            ? Result.Success(value)
            : Result.Failure<T>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue!));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<T> GreaterThan<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (comparer.Compare(value, comparisonValue) <= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue!));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<string> GreaterThan(string value, string comparisonValue, string propertyName)
        => value.CompareTo(comparisonValue) > 0
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<string> GreaterThan(string value, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value.CompareTo(comparisonValue) <= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<byte> GreaterThan(byte value, byte comparisonValue, string propertyName) 
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<byte>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<byte> GreaterThan(byte value, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<short> GreaterThan(short value, short comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<short>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<short> GreaterThan(short value, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<int> GreaterThan(int value, int comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<int>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<int> GreaterThan(int value, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<long> GreaterThan(long value, long comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<long>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<long> GreaterThan(long value, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<float> GreaterThan(float value, float comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<float>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<float> GreaterThan(float value, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<double> GreaterThan(double value, double comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<double>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<double> GreaterThan(double value, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<decimal> GreaterThan(decimal value, decimal comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<decimal>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<decimal> GreaterThan(decimal value, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<BigInteger> GreaterThan(BigInteger value, BigInteger comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<BigInteger> GreaterThan(BigInteger value, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThan(TimeSpan value, TimeSpan comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThan(TimeSpan value, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateOnly> GreaterThan(DateOnly value, DateOnly comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateOnly>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateOnly> GreaterThan(DateOnly value, DateOnly comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTime> GreaterThan(DateTime value, DateTime comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTime>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTime> GreaterThan(DateTime value, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThan(DateTimeOffset value, DateTimeOffset comparisonValue, string propertyName)
        => value > comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThan(DateTimeOffset value, DateTimeOffset comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, GreaterThanError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }
}
