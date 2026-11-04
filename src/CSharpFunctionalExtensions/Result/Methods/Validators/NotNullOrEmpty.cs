#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotNullOrEmpty<T>(IEnumerable<T> value, string propertyName)
    {
        if (value is null)
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (!value.Any())
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotNullOrEmpty<T>(IEnumerable<T> value, PropertyPath path, string member)
    {
        if (value is null)
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (!value.Any())
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(string? value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(string? value, PropertyPath path, string member)
    {
        if (string.IsNullOrEmpty(value))
        {
            string propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }
            
        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(Guid? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == Guid.Empty)
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(Guid? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == Guid.Empty)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(byte? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(byte? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(short? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(short? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(int? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(int? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ushort> NotNullOrEmpty(ushort? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ushort> NotNullOrEmpty(ushort? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<uint> NotNullOrEmpty(uint? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<uint> NotNullOrEmpty(uint? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ulong> NotNullOrEmpty(ulong? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ulong> NotNullOrEmpty(ulong? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(long? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(long? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(float? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(float? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(double? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(double? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(decimal? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(decimal? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(BigInteger? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == BigInteger.Zero)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(BigInteger? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(TimeSpan? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(TimeSpan? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(DateOnly? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(DateOnly? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(DateTime? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(DateTime? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
            
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeOnly> NotNullOrEmpty(TimeOnly? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeOnly> NotNullOrEmpty(TimeOnly? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
            
        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(DateTimeOffset? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (value.Value == default)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(DateTimeOffset? value, PropertyPath path, string member)
    {
        if (!value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        if (value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(value.Value);
    }
}
