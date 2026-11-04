#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotEmpty<T>(IEnumerable<T> value, string propertyName)
        => value is null || !value.Any()
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotEmpty<T>(IEnumerable<T> value, PropertyPath path, string member)
    {
        if (value is null || !value.Any())
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<string> NotEmpty(string? value, string propertyName) 
        => string.IsNullOrEmpty(value)
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<string> NotEmpty(string? value, PropertyPath path, string member)
    {
        if (string.IsNullOrEmpty(value))
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<Guid> NotEmpty(Guid value, string propertyName) 
        => value == Guid.Empty
            ? Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<Guid> NotEmpty(Guid value, PropertyPath path, string member)
    {
        if (value == Guid.Empty)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<byte> NotEmpty(byte value, string propertyName) 
        => value == default
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<byte> NotEmpty(byte value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<short> NotEmpty(short value, string propertyName) 
        => value == default
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<short> NotEmpty(short value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<int> NotEmpty(int value, string propertyName) 
        => value == default
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<int> NotEmpty(int value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ushort> NotEmpty(ushort value, string propertyName) 
        => value == default
            ? Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ushort> NotEmpty(ushort value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<uint> NotEmpty(uint value, string propertyName) 
        => value == default
            ? Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<uint> NotEmpty(uint value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ulong> NotEmpty(ulong value, string propertyName) 
        => value == default
            ? Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ulong> NotEmpty(ulong value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<long> NotEmpty(long value, string propertyName) 
        => value == default
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<long> NotEmpty(long value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<float> NotEmpty(float value, string propertyName) 
        => value == default
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<float> NotEmpty(float value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<double> NotEmpty(double value, string propertyName) 
        => value == default
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<double> NotEmpty(double value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<decimal> NotEmpty(decimal value, string propertyName) 
        => value == default
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<decimal> NotEmpty(decimal value, PropertyPath path, string member)
    {
        if (value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<BigInteger> NotEmpty(BigInteger value, string propertyName)
        => value == BigInteger.Zero
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<BigInteger> NotEmpty(BigInteger value, PropertyPath path, string member)
    {
        if (value == BigInteger.Zero)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeSpan> NotEmpty(TimeSpan value, string propertyName)
        => value == TimeSpan.Zero
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeSpan> NotEmpty(TimeSpan value, PropertyPath path, string member)
    {
        if (value == TimeSpan.Zero)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTime> NotEmpty(DateTime value, string propertyName)
        => value == default(DateTime)
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTime> NotEmpty(DateTime value, PropertyPath path, string member)
    {
        if (value == default(DateTime))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotEmpty(DateTimeOffset value, string propertyName)
        => value == default(DateTimeOffset)
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotEmpty(DateTimeOffset value, PropertyPath path, string member)
    {
        if (value == default(DateTimeOffset))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateOnly> NotEmpty(DateOnly value, string propertyName)
        => value == default(DateOnly)
            ? Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateOnly> NotEmpty(DateOnly value, PropertyPath path, string member)
    {
        if (value == default(DateOnly))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeOnly> NotEmpty(TimeOnly value, string propertyName)
        => value == default(TimeOnly)
            ? Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeOnly> NotEmpty(TimeOnly value, PropertyPath path, string member)
    {
        if (value == default(TimeOnly))
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }
}
