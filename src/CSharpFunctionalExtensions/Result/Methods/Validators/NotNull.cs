#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string NotNullError = "'{0}' must not be empty.";

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<T> NotNull<T>(T? value, string propertyName)
        => value is null
            ? Result.Failure<T>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<T> NotNull<T>(T? value, string propertyNameFormat, params object[] arguments)
    {
        if (value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<IEnumerable<T>> NotNull<T>(IEnumerable<T> value, string propertyName)
        => value is null
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<IEnumerable<T>> NotNull<T>(IEnumerable<T> value, string propertyNameFormat, params object[] arguments)
    {
        if (value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<string> NotNull(string? value, string propertyName) 
        => value is null
            ? Result.Failure<string>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<string> NotNull(string? value, string propertyNameFormat, params object[] arguments)
    {
        if (value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<Guid> NotNull(Guid? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<Guid>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<Guid> NotNull(Guid? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<byte> NotNull(byte? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<byte>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<byte> NotNull(byte? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<short> NotNull(short? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<short>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<short> NotNull(short? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<int> NotNull(int? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<int>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<int> NotNull(int? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<long> NotNull(long? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<long>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<long> NotNull(long? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<float> NotNull(float? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<float>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<float> NotNull(float? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<double> NotNull(double? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<double>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<double> NotNull(double? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<decimal> NotNull(decimal? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<decimal>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<decimal> NotNull(decimal? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<BigInteger> NotNull(BigInteger? value, string propertyName)
        => !value.HasValue
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<BigInteger> NotNull(BigInteger? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeSpan> NotNull(TimeSpan? value, string propertyName)
        => !value.HasValue
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeSpan> NotNull(TimeSpan? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTime> NotNull(DateTime? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<DateTime>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTime> NotNull(DateTime? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTimeOffset> NotNull(DateTimeOffset? value, string propertyName) 
        => !value.HasValue
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotNullError, propertyName))
            : Result.Success(value.Value);

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTimeOffset> NotNull(DateTimeOffset? value, string propertyNameFormat, params object[] arguments)
    {
        if (!value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, NotNullError, propertyName));
        }

        return Result.Success(value.Value);
    }
}
