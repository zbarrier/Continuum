#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<T> NotNull<T>(T? value, string propertyName)
        => value is null
            ? Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<T> NotNull<T>(T? value, PropertyPath path, string member)
    {
        if (value is null)
        {
            var propertyName = path.Format(member);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<IEnumerable<T>> NotNull<T>(IEnumerable<T> value, string propertyName)
        => value is null
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<IEnumerable<T>> NotNull<T>(IEnumerable<T> value, PropertyPath path, string member)
    {
        if (value is null)
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<string> NotNull(string? value, string propertyName) 
        => value is null
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<string> NotNull(string? value, PropertyPath path, string member)
    {
        if (value is null)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<Guid> NotNull(Guid? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<Guid> NotNull(Guid? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<byte> NotNull(byte? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<byte> NotNull(byte? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<short> NotNull(short? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<short> NotNull(short? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<int> NotNull(int? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<int> NotNull(int? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<ushort> NotNull(ushort? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<ushort> NotNull(ushort? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<uint> NotNull(uint? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<uint> NotNull(uint? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<ulong> NotNull(ulong? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<ulong> NotNull(ulong? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<long> NotNull(long? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<long> NotNull(long? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<float> NotNull(float? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<float> NotNull(float? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<double> NotNull(double? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<double> NotNull(double? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<decimal> NotNull(decimal? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<decimal> NotNull(decimal? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<BigInteger> NotNull(BigInteger? value, string propertyName)
        => !value.HasValue
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<BigInteger> NotNull(BigInteger? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeSpan> NotNull(TimeSpan? value, string propertyName)
        => !value.HasValue
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeSpan> NotNull(TimeSpan? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTime> NotNull(DateTime? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTime> NotNull(DateTime? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateOnly> NotNull(DateOnly? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateOnly> NotNull(DateOnly? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeOnly> NotNull(TimeOnly? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeOnly> NotNull(TimeOnly? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTimeOffset> NotNull(DateTimeOffset? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTimeOffset> NotNull(DateTimeOffset? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(value.Value);
    }
}
