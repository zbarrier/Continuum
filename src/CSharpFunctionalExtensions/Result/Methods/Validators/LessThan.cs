#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<T> LessThan<T>(T value, T comparisonValue, IComparer<T> comparer, string propertyName) 
        => comparer.Compare(value, comparisonValue) < 0
            ? Result.Success(value)
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, ErrorArgument.FromObject(comparisonValue)));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<T> LessThan<T>(T value, T comparisonValue, IComparer<T> comparer, PropertyPath path, string member)
    {
        if (comparer.Compare(value, comparisonValue) >= 0)
        {
            var propertyName = path.Format(member);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, ErrorArgument.FromObject(comparisonValue)));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<string> LessThan(string value, string comparisonValue, string propertyName)
        => value.CompareTo(comparisonValue) < 0
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<string> LessThan(string value, string comparisonValue, PropertyPath path, string member)
    {
        if (value.CompareTo(comparisonValue) >= 0)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<byte> LessThan(byte value, byte comparisonValue, string propertyName) 
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<byte> LessThan(byte value, byte comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<short> LessThan(short value, short comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<short> LessThan(short value, short comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<int> LessThan(int value, int comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<int> LessThan(int value, int comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<ushort> LessThan(ushort value, ushort comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<ushort> LessThan(ushort value, ushort comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<uint> LessThan(uint value, uint comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<uint> LessThan(uint value, uint comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<ulong> LessThan(ulong value, ulong comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<ulong> LessThan(ulong value, ulong comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<long> LessThan(long value, long comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<long> LessThan(long value, long comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<float> LessThan(float value, float comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<float> LessThan(float value, float comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<double> LessThan(double value, double comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<double> LessThan(double value, double comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<decimal> LessThan(decimal value, decimal comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<decimal> LessThan(decimal value, decimal comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<BigInteger> LessThan(BigInteger value, BigInteger comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<BigInteger> LessThan(BigInteger value, BigInteger comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<TimeSpan> LessThan(TimeSpan value, TimeSpan comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<TimeSpan> LessThan(TimeSpan value, TimeSpan comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTime> LessThan(DateTime value, DateTime comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTime> LessThan(DateTime value, DateTime comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateOnly> LessThan(DateOnly value, DateOnly comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateOnly> LessThan(DateOnly value, DateOnly comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<TimeOnly> LessThan(TimeOnly value, TimeOnly comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<TimeOnly> LessThan(TimeOnly value, TimeOnly comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThan(DateTimeOffset value, DateTimeOffset comparisonValue, string propertyName)
        => value < comparisonValue
            ? Result.Success(value)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));

    /// <summary>
    ///     Ensure value is less than given value.
    /// </summary>
    public static Result<DateTimeOffset> LessThan(DateTimeOffset value, DateTimeOffset comparisonValue, PropertyPath path, string member)
    {
        if (value >= comparisonValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, propertyName, comparisonValue));
        }

        return Result.Success(value);
    }
}
