#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string LessThanOrEqualError = "'{0}' must be less than or equal to '{1}'.";

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<T> LessThanOrEqual<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyName) 
        => comparer.Compare(value, comparisonValue) <= 0
            ? Result.Success(value)
            : Result.Failure<T>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue!));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<T> LessThanOrEqual<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (comparer.Compare(value, comparisonValue) > 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue!));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<string> LessThanOrEqual(string value, string comparisonValue, string propertyName)
        => value.CompareTo(comparisonValue) <= 0
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<string> LessThanOrEqual(string value, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value.CompareTo(comparisonValue) > 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<byte> LessThanOrEqual(byte value, byte comparisonValue, string propertyName) 
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<byte>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<byte> LessThanOrEqual(byte value, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<short> LessThanOrEqual(short value, short comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<short>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<short> LessThanOrEqual(short value, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<int> LessThanOrEqual(int value, int comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<int>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<int> LessThanOrEqual(int value, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<long> LessThanOrEqual(long value, long comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<long>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<long> LessThanOrEqual(long value, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<float> LessThanOrEqual(float value, float comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<float>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<float> LessThanOrEqual(float value, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<double> LessThanOrEqual(double value, double comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<double>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<double> LessThanOrEqual(double value, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<decimal> LessThanOrEqual(decimal value, decimal comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<decimal>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<decimal> LessThanOrEqual(decimal value, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<BigInteger> LessThanOrEqual(BigInteger value, BigInteger comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<BigInteger> LessThanOrEqual(BigInteger value, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> LessThanOrEqual(TimeSpan value, TimeSpan comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> LessThanOrEqual(TimeSpan value, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTime> LessThanOrEqual(DateTime value, DateTime comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTime>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTime> LessThanOrEqual(DateTime value, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThanOrEqual(DateTimeOffset value, DateTimeOffset comparisonValue, string propertyName)
        => value <= comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThanOrEqual(DateTimeOffset value, DateTimeOffset comparisonValue, 
        string propertyNameFormat, params object[] arguments)
    {
        if (value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, LessThanOrEqualError, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }
}
