#nullable enable

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<T> InclusiveBetween<T>(this Result<T> result, T from, T to, IComparer<T> comparer, string propertyName)
        where T : class
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(to, from) == -1)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return comparer.Compare(result.Value, from) < 0 || comparer.Compare(result.Value, to) > 0
            ? Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(result.Value)))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<T> InclusiveBetween<T>(this Result<T> result, T from, T to, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
        where T : class
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(to, from) == -1)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (comparer.Compare(result.Value, from) < 0 || comparer.Compare(result.Value, to) > 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(result.Value)));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<string> InclusiveBetween(this Result<string> result, string from, string to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from.CompareTo(to) >= 0)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value.CompareTo(from) < 0 || result.Value.CompareTo(to) > 0
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(result.Value)))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<string> InclusiveBetween(this Result<string> result, string from, string to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from.CompareTo(to) >= 0)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value.CompareTo(from) < 0 || result.Value.CompareTo(to) > 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(result.Value)));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<byte> InclusiveBetween(this Result<byte> result, byte from, byte to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<byte> InclusiveBetween(this Result<byte> result, byte from, byte to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<short> InclusiveBetween(this Result<short> result, short from, short to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<short> InclusiveBetween(this Result<short> result, short from, short to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<int> InclusiveBetween(this Result<int> result, int from, int to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<int> InclusiveBetween(this Result<int> result, int from, int to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<long> InclusiveBetween(this Result<long> result, long from, long to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<long> InclusiveBetween(this Result<long> result, long from, long to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<float> InclusiveBetween(this Result<float> result, float from, float to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<float> InclusiveBetween(this Result<float> result, float from, float to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<double> InclusiveBetween(this Result<double> result, double from, double to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<double> InclusiveBetween(this Result<double> result, double from, double to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<decimal> InclusiveBetween(this Result<decimal> result, decimal from, decimal to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<decimal> InclusiveBetween(this Result<decimal> result, decimal from, decimal to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<BigInteger> InclusiveBetween(this Result<BigInteger> result, BigInteger from, BigInteger to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<BigInteger> InclusiveBetween(this Result<BigInteger> result, BigInteger from, BigInteger to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<TimeSpan> InclusiveBetween(this Result<TimeSpan> result, TimeSpan from, TimeSpan to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<TimeSpan> InclusiveBetween(this Result<TimeSpan> result, TimeSpan from, TimeSpan to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTime> InclusiveBetween(this Result<DateTime> result, DateTime from, DateTime to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to) 
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTime> InclusiveBetween(this Result<DateTime> result, DateTime from, DateTime to, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTimeOffset> InclusiveBetween(this Result<DateTimeOffset> result, DateTimeOffset from, DateTimeOffset to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTimeOffset> InclusiveBetween(this Result<DateTimeOffset> result, DateTimeOffset from, DateTimeOffset to, 
        string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }
}
