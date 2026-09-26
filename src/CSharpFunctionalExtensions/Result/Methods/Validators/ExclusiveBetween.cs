#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string ExclusiveBetweenError = "'{0}' must be between {1} and {2} (exclusive). You entered {3}.";
    const string ToShouldBeLargerThanFrom = "To should be larger than from.";

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<T> ExclusiveBetween<T>(T value, T from, T to, IComparer<T> comparer, string propertyName)
    {
        if (comparer.Compare(to, from) == -1)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return comparer.Compare(value, from) <= 0 || comparer.Compare(value, to) >= 0
            ? Result.Failure<T>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from!, to!, value!))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<T> ExclusiveBetween<T>(T value, T from, T to, IComparer<T> comparer, string propertyNameFormat, params object[] arguments)
    {
        if (comparer.Compare(to, from) == -1)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (comparer.Compare(value, from) <= 0 || comparer.Compare(value, to) >= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from!, to!, value!));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<string> ExclusiveBetween(string value, string from, string to, string propertyName)
    {
        if (from.CompareTo(to) >= 0)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value.CompareTo(from) <= 0 || value.CompareTo(to) >= 0
            ? Result.Failure<string>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from!, to!, value!))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<string> ExclusiveBetween(string value, string from, string to, string propertyNameFormat, params object[] arguments)
    {
        if (from.CompareTo(to) >= 0)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value.CompareTo(from) <= 0 || value.CompareTo(to) >= 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from!, to!, value!));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<byte> ExclusiveBetween(byte value, byte from, byte to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<byte>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<byte> ExclusiveBetween(byte value, byte from, byte to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<short> ExclusiveBetween(short value, short from, short to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<short>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<short> ExclusiveBetween(short value, short from, short to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<int> ExclusiveBetween(int value, int from, int to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<int>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<int> ExclusiveBetween(int value, int from, int to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<long> ExclusiveBetween(long value, long from, long to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<long>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<long> ExclusiveBetween(long value, long from, long to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<float> ExclusiveBetween(float value, float from, float to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<float>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<float> ExclusiveBetween(float value, float from, float to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<double> ExclusiveBetween(double value, double from, double to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<double>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<double> ExclusiveBetween(double value, double from, double to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<decimal> ExclusiveBetween(decimal value, decimal from, decimal to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<decimal>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<decimal> ExclusiveBetween(decimal value, decimal from, decimal to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<BigInteger> ExclusiveBetween(BigInteger value, BigInteger from, BigInteger to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<BigInteger> ExclusiveBetween(BigInteger value, BigInteger from, BigInteger to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<TimeSpan> ExclusiveBetween(TimeSpan value, TimeSpan from, TimeSpan to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<TimeSpan> ExclusiveBetween(TimeSpan value, TimeSpan from, TimeSpan to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTime> ExclusiveBetween(DateTime value, DateTime from, DateTime to, string propertyName)
    {
        if (from >= to) 
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTime> ExclusiveBetween(DateTime value, DateTime from, DateTime to, string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTimeOffset> ExclusiveBetween(DateTimeOffset value, DateTimeOffset from, DateTimeOffset to, string propertyName)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        return value <= from || value >= to
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is between given range (exclusive).
    /// </summary>
    public static Result<DateTimeOffset> ExclusiveBetween(DateTimeOffset value, DateTimeOffset from, DateTimeOffset to, 
        string propertyNameFormat, params object[] arguments)
    {
        if (from >= to)
            throw new ArgumentOutOfRangeException(ToShouldBeLargerThanFrom);

        if (value <= from || value >= to)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ExclusiveBetweenError, propertyName, from, to, value));
        }

        return Result.Success(value);
    }
}
