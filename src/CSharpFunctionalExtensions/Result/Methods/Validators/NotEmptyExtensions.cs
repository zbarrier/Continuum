#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotEmpty<T>(this Result<IEnumerable<T>> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value is null || !result.Value.Any()
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<IEnumerable<T>> NotEmpty<T>(this Result<IEnumerable<T>> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value is null || !result.Value.Any())
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<string> NotEmpty(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return string.IsNullOrEmpty(result.Value)
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<string> NotEmpty(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (string.IsNullOrEmpty(result.Value))
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<Guid> NotEmpty(this Result<Guid> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == Guid.Empty
            ? Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<Guid> NotEmpty(this Result<Guid> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == Guid.Empty)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<byte> NotEmpty(this Result<byte> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<byte> NotEmpty(this Result<byte> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<short> NotEmpty(this Result<short> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<short> NotEmpty(this Result<short> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<int> NotEmpty(this Result<int> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<int> NotEmpty(this Result<int> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ushort> NotEmpty(this Result<ushort> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ushort> NotEmpty(this Result<ushort> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<uint> NotEmpty(this Result<uint> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<uint> NotEmpty(this Result<uint> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ulong> NotEmpty(this Result<ulong> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<ulong> NotEmpty(this Result<ulong> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<long> NotEmpty(this Result<long> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<long> NotEmpty(this Result<long> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<float> NotEmpty(this Result<float> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<float> NotEmpty(this Result<float> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<double> NotEmpty(this Result<double> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<double> NotEmpty(this Result<double> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<decimal> NotEmpty(this Result<decimal> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default
            ? Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<decimal> NotEmpty(this Result<decimal> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<BigInteger> NotEmpty(this Result<BigInteger> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == BigInteger.Zero
            ? Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<BigInteger> NotEmpty(this Result<BigInteger> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == BigInteger.Zero)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeSpan> NotEmpty(this Result<TimeSpan> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == TimeSpan.Zero
            ? Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeSpan> NotEmpty(this Result<TimeSpan> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == TimeSpan.Zero)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTime> NotEmpty(this Result<DateTime> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default(DateTime)
            ? Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTime> NotEmpty(this Result<DateTime> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default(DateTime))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotEmpty(this Result<DateTimeOffset> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default(DateTimeOffset)
            ? Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateTimeOffset> NotEmpty(this Result<DateTimeOffset> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default(DateTimeOffset))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateOnly> NotEmpty(this Result<DateOnly> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default(DateOnly)
            ? Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<DateOnly> NotEmpty(this Result<DateOnly> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default(DateOnly))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeOnly> NotEmpty(this Result<TimeOnly> result, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value == default(TimeOnly)
            ? Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : result;
    }

    /// <summary>
    ///     Ensure value is not empty or default.
    /// </summary>
    public static Result<TimeOnly> NotEmpty(this Result<TimeOnly> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == default(TimeOnly))
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName));
        }

        return result;
    }
}
