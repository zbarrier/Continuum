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

        if (comparer.Compare(to, from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return comparer.Compare(result.Value, from) < 0 || comparer.Compare(result.Value, to) > 0
            ? Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(result.Value)))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<T> InclusiveBetween<T>(this Result<T> result, T from, T to, IComparer<T> comparer, PropertyPath path, string member)
        where T : class
    {
        if (result.IsFailure) return result;

        if (comparer.Compare(to, from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (comparer.Compare(result.Value, from) < 0 || comparer.Compare(result.Value, to) > 0)
        {
            var propertyName = path.Format(member);
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

        if (to.CompareTo(from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value.CompareTo(from) < 0 || result.Value.CompareTo(to) > 0
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(result.Value)))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<string> InclusiveBetween(this Result<string> result, string from, string to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (to.CompareTo(from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value.CompareTo(from) < 0 || result.Value.CompareTo(to) > 0)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<byte> InclusiveBetween(this Result<byte> result, byte from, byte to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<short> InclusiveBetween(this Result<short> result, short from, short to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<int> InclusiveBetween(this Result<int> result, int from, int to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<ushort> InclusiveBetween(this Result<ushort> result, ushort from, ushort to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<ushort> InclusiveBetween(this Result<ushort> result, ushort from, ushort to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<uint> InclusiveBetween(this Result<uint> result, uint from, uint to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<uint> InclusiveBetween(this Result<uint> result, uint from, uint to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<ulong> InclusiveBetween(this Result<ulong> result, ulong from, ulong to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<ulong> InclusiveBetween(this Result<ulong> result, ulong from, ulong to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<long> InclusiveBetween(this Result<long> result, long from, long to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<long> InclusiveBetween(this Result<long> result, long from, long to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<float> InclusiveBetween(this Result<float> result, float from, float to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<double> InclusiveBetween(this Result<double> result, double from, double to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<decimal> InclusiveBetween(this Result<decimal> result, decimal from, decimal to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<BigInteger> InclusiveBetween(this Result<BigInteger> result, BigInteger from, BigInteger to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<TimeSpan> InclusiveBetween(this Result<TimeSpan> result, TimeSpan from, TimeSpan to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
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

        if (from > to) 
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTime> InclusiveBetween(this Result<DateTime> result, DateTime from, DateTime to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateOnly> InclusiveBetween(this Result<DateOnly> result, DateOnly from, DateOnly to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to) 
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateOnly> InclusiveBetween(this Result<DateOnly> result, DateOnly from, DateOnly to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<TimeOnly> InclusiveBetween(this Result<TimeOnly> result, TimeOnly from, TimeOnly to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to) 
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<TimeOnly> InclusiveBetween(this Result<TimeOnly> result, TimeOnly from, TimeOnly to, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTimeOffset> InclusiveBetween(this Result<DateTimeOffset> result, DateTimeOffset from, DateTimeOffset to, string propertyName)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return result.Value < from || result.Value > to
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value))
            : result;
    }

    /// <summary>
    ///     Ensure value is between given range.
    /// </summary>
    public static Result<DateTimeOffset> InclusiveBetween(this Result<DateTimeOffset> result, DateTimeOffset from, DateTimeOffset to, 
        PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (result.Value < from || result.Value > to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InclusiveBetween, ValidatorErrorStrings.InclusiveBetween, propertyName, from, to, result.Value));
        }

        return result;
    }
}
