#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotNullOrEmpty<T>(this Result<IEnumerable<T>> result, string propertyName)
    {
        if (result.IsFailure) return result;

        if (result.Value is null)
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (!result.Value.Any())
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotNullOrEmpty<T>(this Result<IEnumerable<T>> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value is null)
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (!result.Value.Any())
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        if (string.IsNullOrEmpty(result.Value))
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (string.IsNullOrEmpty(result.Value))
        {
            string propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }
            
        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(this Result<Guid?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == Guid.Empty)
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(this Result<Guid?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == Guid.Empty)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(this Result<byte?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(this Result<byte?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(this Result<short?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(this Result<short?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(this Result<int?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(this Result<int?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ushort> NotNullOrEmpty(this Result<ushort?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<ushort>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ushort> NotNullOrEmpty(this Result<ushort?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<ushort>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<uint> NotNullOrEmpty(this Result<uint?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<uint>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<uint> NotNullOrEmpty(this Result<uint?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<uint>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ulong> NotNullOrEmpty(this Result<ulong?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<ulong>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<ulong> NotNullOrEmpty(this Result<ulong?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<ulong>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(this Result<long?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(this Result<long?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(this Result<float?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(this Result<float?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(this Result<double?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(this Result<double?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(this Result<decimal?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(this Result<decimal?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(this Result<BigInteger?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == BigInteger.Zero)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(this Result<BigInteger?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(this Result<TimeSpan?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(this Result<TimeSpan?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(this Result<DateOnly?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(this Result<DateOnly?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(this Result<DateTime?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(this Result<DateTime?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
            
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeOnly> NotNullOrEmpty(this Result<TimeOnly?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<TimeOnly>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeOnly> NotNullOrEmpty(this Result<TimeOnly?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<TimeOnly>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }
            
        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(this Result<DateTimeOffset?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        if (!result.Value.HasValue)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(this Result<DateTimeOffset?> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        if (result.Value.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return Result.Success(result.Value.Value);
    }
}
