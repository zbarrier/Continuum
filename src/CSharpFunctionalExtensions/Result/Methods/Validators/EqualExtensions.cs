#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const string EqualError = "'{0}' must be equal to '{1}'.";

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<T> Equal<T>(this Result<T> result, T expectedValue, IEqualityComparer<T> comparer, string propertyName)
    {
        if (result.IsFailure) return result;

        bool success = comparer is not null
            ? comparer.Equals(result.Value, expectedValue)
            : Equals(result.Value, expectedValue);

        return success
            ? result
            : Result.Failure<T>(new ValidationError(propertyName, EqualError, propertyName, expectedValue!));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<T> Equal<T>(this Result<T> result, T expectedValue, IEqualityComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        bool success = comparer is not null
            ? comparer.Equals(result.Value, expectedValue)
            : Equals(result.Value, expectedValue);

        if (!success)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, EqualError, propertyName, expectedValue!));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<string> Equal(this Result<string> result, string expectedValue, StringComparison stringComparisonType, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Equals(expectedValue, stringComparisonType)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<string> Equal(this Result<string> result, string expectedValue, StringComparison stringComparisonType,
        string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (!result.Value.Equals(expectedValue, stringComparisonType))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<byte> Equal(this Result<byte> result, byte expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<byte>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<byte> Equal(this Result<byte> result, byte expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<short> Equal(this Result<short> result, short expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<short>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<short> Equal(this Result<short> result, short expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<int> Equal(this Result<int> result, int expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<int>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<int> Equal(this Result<int> result, int expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<long> Equal(this Result<long> result, long expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<long>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<long> Equal(this Result<long> result, long expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<float> Equal(this Result<float> result, float expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<float>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<float> Equal(this Result<float> result, float expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<double> Equal(this Result<double> result, double expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<double>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<double> Equal(this Result<double> result, double expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<decimal> Equal(this Result<decimal> result, decimal expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<decimal>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<decimal> Equal(this Result<decimal> result, decimal expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<BigInteger> Equal(this Result<BigInteger> result, BigInteger expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<BigInteger>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<BigInteger> Equal(this Result<BigInteger> result, BigInteger expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<TimeSpan> Equal(this Result<TimeSpan> result, TimeSpan expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<TimeSpan> Equal(this Result<TimeSpan> result, TimeSpan expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTime> Equal(this Result<DateTime> result, DateTime expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<DateTime>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTime> Equal(this Result<DateTime> result, DateTime expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> Equal(this Result<DateTimeOffset> result, DateTimeOffset expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == expectedValue
            ? result
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> Equal(this Result<DateTimeOffset> result, DateTimeOffset expectedValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value != expectedValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, EqualError, propertyName, expectedValue));
        }

        return result;
    }
}
