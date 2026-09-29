#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<T> GreaterThan<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyName)
    {
        if (result.IsFailure) return result;

        return comparer.Compare(result.Value, comparisonValue) > 0
            ? result
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, ErrorArgument.FromObject(comparisonValue)));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<T> GreaterThan<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(result.Value, comparisonValue) <= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, ErrorArgument.FromObject(comparisonValue)));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<string> GreaterThan(this Result<string> result, string comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.CompareTo(comparisonValue) > 0
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<string> GreaterThan(this Result<string> result, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.CompareTo(comparisonValue) <= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<byte> GreaterThan(this Result<byte> result, byte comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<byte> GreaterThan(this Result<byte> result, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<short> GreaterThan(this Result<short> result, short comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<short> GreaterThan(this Result<short> result, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<int> GreaterThan(this Result<int> result, int comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<int> GreaterThan(this Result<int> result, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<long> GreaterThan(this Result<long> result, long comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<long> GreaterThan(this Result<long> result, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<float> GreaterThan(this Result<float> result, float comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<float> GreaterThan(this Result<float> result, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<double> GreaterThan(this Result<double> result, double comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<double> GreaterThan(this Result<double> result, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<decimal> GreaterThan(this Result<decimal> result, decimal comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<decimal> GreaterThan(this Result<decimal> result, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<BigInteger> GreaterThan(this Result<BigInteger> result, BigInteger comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<BigInteger> GreaterThan(this Result<BigInteger> result, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThan(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<TimeSpan> GreaterThan(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateOnly> GreaterThan(this Result<DateOnly> result, DateOnly comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateOnly> GreaterThan(this Result<DateOnly> result, DateOnly comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTime> GreaterThan(this Result<DateTime> result, DateTime comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTime> GreaterThan(this Result<DateTime> result, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThan(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value > comparisonValue
            ? result
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is greater than given value.
    /// </summary>
    public static Result<DateTimeOffset> GreaterThan(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value <= comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, propertyName, comparisonValue));
        }

        return result;
    }
}
