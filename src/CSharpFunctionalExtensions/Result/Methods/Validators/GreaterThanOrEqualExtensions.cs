#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<T> GreaterThanOrEqual<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyName)
    {
        if (result.IsFailure) return result;

        return comparer.Compare(result.Value, comparisonValue) >= 0
            ? result
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, ErrorArgument.FromObject(comparisonValue)));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<T> GreaterThanOrEqual<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(result.Value, comparisonValue) < 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, ErrorArgument.FromObject(comparisonValue)));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<string> GreaterThanOrEqual(this Result<string> result, string comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.CompareTo(comparisonValue) >= 0
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<string> GreaterThanOrEqual(this Result<string> result, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.CompareTo(comparisonValue) < 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<byte> GreaterThanOrEqual(this Result<byte> result, byte comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<byte> GreaterThanOrEqual(this Result<byte> result, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<short> GreaterThanOrEqual(this Result<short> result, short comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<short> GreaterThanOrEqual(this Result<short> result, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<int> GreaterThanOrEqual(this Result<int> result, int comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<int> GreaterThanOrEqual(this Result<int> result, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<long> GreaterThanOrEqual(this Result<long> result, long comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<long> GreaterThanOrEqual(this Result<long> result, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<float> GreaterThanOrEqual(this Result<float> result, float comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<float> GreaterThanOrEqual(this Result<float> result, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<double> GreaterThanOrEqual(this Result<double> result, double comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<double> GreaterThanOrEqual(this Result<double> result, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<decimal> GreaterThanOrEqual(this Result<decimal> result, decimal comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<decimal> GreaterThanOrEqual(this Result<decimal> result, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<BigInteger> GreaterThanOrEqual(this Result<BigInteger> result, BigInteger comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<BigInteger> GreaterThanOrEqual(this Result<BigInteger> result, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThanOrEqual(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThanOrEqual(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateOnly> GreaterThanOrEqual(this Result<DateOnly> result, DateOnly comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateOnly> GreaterThanOrEqual(this Result<DateOnly> result, DateOnly comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTime> GreaterThanOrEqual(this Result<DateTime> result, DateTime comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTime> GreaterThanOrEqual(this Result<DateTime> result, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThanOrEqual(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value >= comparisonValue
            ? result
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThanOrEqual(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, 
        string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value < comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }
}
