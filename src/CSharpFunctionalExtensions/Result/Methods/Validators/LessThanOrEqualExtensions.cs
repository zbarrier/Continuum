#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<T> LessThanOrEqual<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyName)
    {
        if (result.IsFailure) return result;

        return comparer.Compare(result.Value, comparisonValue) <= 0
            ? result
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, ErrorArgument.FromObject(comparisonValue)));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<T> LessThanOrEqual<T>(this Result<T> result, T comparisonValue, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(result.Value, comparisonValue) > 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, ErrorArgument.FromObject(comparisonValue)));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<string> LessThanOrEqual(this Result<string> result, string comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.CompareTo(comparisonValue) <= 0
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<string> LessThanOrEqual(this Result<string> result, string comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.CompareTo(comparisonValue) > 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<byte> LessThanOrEqual(this Result<byte> result, byte comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<byte> LessThanOrEqual(this Result<byte> result, byte comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<short> LessThanOrEqual(this Result<short> result, short comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<short> LessThanOrEqual(this Result<short> result, short comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<int> LessThanOrEqual(this Result<int> result, int comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<int> LessThanOrEqual(this Result<int> result, int comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<long> LessThanOrEqual(this Result<long> result, long comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<long> LessThanOrEqual(this Result<long> result, long comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<float> LessThanOrEqual(this Result<float> result, float comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<float> LessThanOrEqual(this Result<float> result, float comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<double> LessThanOrEqual(this Result<double> result, double comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<double> LessThanOrEqual(this Result<double> result, double comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<decimal> LessThanOrEqual(this Result<decimal> result, decimal comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<decimal> LessThanOrEqual(this Result<decimal> result, decimal comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<BigInteger> LessThanOrEqual(this Result<BigInteger> result, BigInteger comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<BigInteger> LessThanOrEqual(this Result<BigInteger> result, BigInteger comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> LessThanOrEqual(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<TimeSpan> LessThanOrEqual(this Result<TimeSpan> result, TimeSpan comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTime> LessThanOrEqual(this Result<DateTime> result, DateTime comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTime> LessThanOrEqual(this Result<DateTime> result, DateTime comparisonValue, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThanOrEqual(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value <= comparisonValue
            ? result
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
    }

    /// <summary>
    ///     Ensure value is less than or equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThanOrEqual(this Result<DateTimeOffset> result, DateTimeOffset comparisonValue, 
        string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value > comparisonValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, propertyName, comparisonValue));
        }

        return result;
    }
}
