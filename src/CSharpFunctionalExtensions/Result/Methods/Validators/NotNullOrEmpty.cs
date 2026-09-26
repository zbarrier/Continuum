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
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotNullError, propertyName));
        if (!value.Any())
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotNullOrEmpty<T>(IEnumerable<T> value, string propertyNameFormat, params object[] arguments)
    {
        if (value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (!value.Any())
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(string? value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
            return Result.Failure<string>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<string> NotNullOrEmpty(string? value, string propertyNameFormat, params object[] arguments)
    {
        if (string.IsNullOrEmpty(value))
        {
            string propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }
            
        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(Guid? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<Guid>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == Guid.Empty)
            return Result.Failure<Guid>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<Guid> NotNullOrEmpty(Guid? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == Guid.Empty)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(byte? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<byte>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<byte>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<byte> NotNullOrEmpty(byte? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(short? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<short>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<short>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<short> NotNullOrEmpty(short? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(int? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<int>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<int>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<int> NotNullOrEmpty(int? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(long? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<long>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<long>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<long> NotNullOrEmpty(long? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(float? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<float>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<float>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<float> NotNullOrEmpty(float? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(double? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<double>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<double>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<double> NotNullOrEmpty(double? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(decimal? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<decimal>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<decimal>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<decimal> NotNullOrEmpty(decimal? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(BigInteger? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == BigInteger.Zero)
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<BigInteger> NotNullOrEmpty(BigInteger? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotNullError, propertyName));
        }
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(TimeSpan? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<TimeSpan> NotNullOrEmpty(TimeSpan? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(DateOnly? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateOnly> NotNullOrEmpty(DateOnly? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(DateTime? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTime> NotNullOrEmpty(DateTime? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotNullError, propertyName));
        }
            
        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(DateTimeOffset? value, string propertyName)
    {
        if (!value.HasValue)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotNullError, propertyName));
        if (value.Value == default)
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotEmptyError, propertyName));

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null, empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotNullOrEmpty(DateTimeOffset? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        if (value.Value == default)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotEmptyError, propertyName));
        }

        return Result.Success(value.Value);
    }
}
