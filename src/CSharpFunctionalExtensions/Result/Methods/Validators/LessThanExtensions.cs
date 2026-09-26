#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const string LessThanError = "'{0}' must be less than '{1}'.";

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<T> LessThan<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyName)
    {
        if (result.IsFailure) return result;

        return comparer.Compare(result.Value, comparisonValue) < 0
            ? result
            : Result.Failure<T>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue!));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<T> LessThan<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(result.Value, comparisonValue) >= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue!));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<string> LessThan(this Result<string> result, string comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.CompareTo(comparisonValue) < 0
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<string> LessThan(this Result<string> result, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.CompareTo(comparisonValue) >= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<byte> LessThan(this Result<byte> result, byte comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<byte>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<byte> LessThan(this Result<byte> result, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<short> LessThan(this Result<short> result, short comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<short>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<short> LessThan(this Result<short> result, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<int> LessThan(this Result<int> result, int comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<int>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<int> LessThan(this Result<int> result, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<long> LessThan(this Result<long> result, long comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<long>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<long> LessThan(this Result<long> result, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<float> LessThan(this Result<float> result, float comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<float>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<float> LessThan(this Result<float> result, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<double> LessThan(this Result<double> result, double comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<double>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<double> LessThan(this Result<double> result, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<decimal> LessThan(this Result<decimal> result, decimal comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<decimal>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<decimal> LessThan(this Result<decimal> result, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<BigInteger> LessThan(this Result<BigInteger> result, BigInteger comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<BigInteger>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<BigInteger> LessThan(this Result<BigInteger> result, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<TimeSpan> LessThan(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<TimeSpan> LessThan(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTime> LessThan(this Result<DateTime> result, DateTime comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<DateTime>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTime> LessThan(this Result<DateTime> result, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThan(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value < comparisonValue
            ? result
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThan(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value >= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, LessThanError, propertyName, comparisonValue));
        }

        return result;
    }
}
