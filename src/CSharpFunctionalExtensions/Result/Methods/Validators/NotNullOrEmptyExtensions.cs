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
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotNullError, propertyName));
        if (!result.Value.Any())
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotNullOrEmpty<T>(this Result<IEnumerable<T>> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (!result.Value.Any())
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<string>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (string.IsNullOrEmpty(result.Value))
        {
            string propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<Guid>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == Guid.Empty)
            return Result.Failure<Guid>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(this Result<Guid?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == Guid.Empty)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<byte>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<byte>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(this Result<byte?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<short>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<short>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(this Result<short?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<int>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<int>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(this Result<int?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<long>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<long>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(this Result<long?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<float>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<float>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(this Result<float?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<double>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<double>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(this Result<double?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<decimal>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<decimal>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(this Result<decimal?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == BigInteger.Zero)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(this Result<BigInteger?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(this Result<TimeSpan?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(this Result<DateOnly?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(this Result<DateTime?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotNullError, propertyName));
        }
            
        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotEmptyError, propertyName));
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
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotNullError, propertyName));
        if (result.Value.Value == default)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(this Result<DateTimeOffset?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        if (result.Value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(result.Value.Value);
    }
}
