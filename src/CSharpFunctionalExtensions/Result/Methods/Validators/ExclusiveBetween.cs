#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string ToShouldBeLargerThanFrom = "To should be larger than from.";

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<T> ExclusiveBetween<T>(T value, T from, T to, IComparer<T> comparer, string propertyName)
    {
        if (comparer.Compare(to, from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return comparer.Compare(value, from) <= 0 || comparer.Compare(value, to) >= 0
            ? Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(value)))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<T> ExclusiveBetween<T>(T value, T from, T to, IComparer<T> comparer, PropertyPath path, string member)
    {
        if (comparer.Compare(to, from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (comparer.Compare(value, from) <= 0 || comparer.Compare(value, to) >= 0)
        {
            var propertyName = path.Format(member);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(value)));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<string> ExclusiveBetween(string value, string from, string to, string propertyName)
    {
        if (to.CompareTo(from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value.CompareTo(from) <= 0 || value.CompareTo(to) >= 0
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(value)))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<string> ExclusiveBetween(string value, string from, string to, PropertyPath path, string member)
    {
        if (to.CompareTo(from) < 0)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value.CompareTo(from) <= 0 || value.CompareTo(to) >= 0)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, ErrorArgument.FromObject(from), ErrorArgument.FromObject(to), ErrorArgument.FromObject(value)));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<byte> ExclusiveBetween(byte value, byte from, byte to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<byte> ExclusiveBetween(byte value, byte from, byte to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<short> ExclusiveBetween(short value, short from, short to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<short> ExclusiveBetween(short value, short from, short to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<int> ExclusiveBetween(int value, int from, int to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<int> ExclusiveBetween(int value, int from, int to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<ushort> ExclusiveBetween(ushort value, ushort from, ushort to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<ushort> ExclusiveBetween(ushort value, ushort from, ushort to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<uint> ExclusiveBetween(uint value, uint from, uint to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<uint> ExclusiveBetween(uint value, uint from, uint to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<ulong> ExclusiveBetween(ulong value, ulong from, ulong to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<ulong> ExclusiveBetween(ulong value, ulong from, ulong to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<long> ExclusiveBetween(long value, long from, long to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<long> ExclusiveBetween(long value, long from, long to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<float> ExclusiveBetween(float value, float from, float to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<float> ExclusiveBetween(float value, float from, float to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<double> ExclusiveBetween(double value, double from, double to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<double> ExclusiveBetween(double value, double from, double to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<decimal> ExclusiveBetween(decimal value, decimal from, decimal to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<decimal> ExclusiveBetween(decimal value, decimal from, decimal to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<BigInteger> ExclusiveBetween(BigInteger value, BigInteger from, BigInteger to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<BigInteger> ExclusiveBetween(BigInteger value, BigInteger from, BigInteger to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<TimeSpan> ExclusiveBetween(TimeSpan value, TimeSpan from, TimeSpan to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<TimeSpan> ExclusiveBetween(TimeSpan value, TimeSpan from, TimeSpan to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTime> ExclusiveBetween(DateTime value, DateTime from, DateTime to, string propertyName)
    {
        if (from > to) 
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTime> ExclusiveBetween(DateTime value, DateTime from, DateTime to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateOnly> ExclusiveBetween(DateOnly value, DateOnly from, DateOnly to, string propertyName)
    {
        if (from > to) 
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateOnly> ExclusiveBetween(DateOnly value, DateOnly from, DateOnly to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<TimeOnly> ExclusiveBetween(TimeOnly value, TimeOnly from, TimeOnly to, string propertyName)
    {
        if (from > to) 
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<TimeOnly> ExclusiveBetween(TimeOnly value, TimeOnly from, TimeOnly to, PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTimeOffset> ExclusiveBetween(DateTimeOffset value, DateTimeOffset from, DateTimeOffset to, string propertyName)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTimeOffset> ExclusiveBetween(DateTimeOffset value, DateTimeOffset from, DateTimeOffset to, 
        PropertyPath path, string member)
    {
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(to), ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.ExclusiveBetween, ValidatorErrorStrings.ExclusiveBetween, propertyName, from, to, value));
        }

        return Result.Success(value);
    }
}
