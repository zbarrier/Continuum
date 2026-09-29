#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<T> NotNull<T>(this Result<T> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value is null
            ? Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<T> NotNull<T>(this Result<T> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<IEnumerable<T>> NotNull<T>(this Result<IEnumerable<T>> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value is null
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<IEnumerable<T>> NotNull<T>(this Result<IEnumerable<T>> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<string> NotNull(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value is null
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<string> NotNull(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value is null)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<Guid> NotNull(this Result<Guid?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<Guid> NotNull(this Result<Guid?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<byte> NotNull(this Result<byte?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<byte> NotNull(this Result<byte?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<short> NotNull(this Result<short?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<short> NotNull(this Result<short?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<int> NotNull(this Result<int?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<int> NotNull(this Result<int?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<long> NotNull(this Result<long?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<long> NotNull(this Result<long?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<float> NotNull(this Result<float?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<float> NotNull(this Result<float?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<double> NotNull(this Result<double?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<double> NotNull(this Result<double?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<decimal> NotNull(this Result<decimal?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<decimal> NotNull(this Result<decimal?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<BigInteger> NotNull(this Result<BigInteger?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<BigInteger> NotNull(this Result<BigInteger?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeSpan> NotNull(this Result<TimeSpan?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<TimeSpan> NotNull(this Result<TimeSpan?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTime> NotNull(this Result<DateTime?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTime> NotNull(this Result<DateTime?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTimeOffset> NotNull(this Result<DateTimeOffset?> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        return !result.Value.HasValue
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName))
            : Result.Success(result.Value.Value);
    }

    /// <summary>
    ///     Ensure value is not null.
    /// </summary>
    public static Result<DateTimeOffset> NotNull(this Result<DateTimeOffset?> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        if (!result.Value.HasValue)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotNull, ValidatorErrorStrings.NotNull, propertyName));
        }

        return Result.Success(result.Value.Value);
    }
}
