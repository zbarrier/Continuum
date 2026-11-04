#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<T> NotEqual<T>(this Result<T> result, T expectedValue, IEqualityComparer<T> comparer, string propertyName)
    {
        if (result.IsFailure) return result;

        bool success = comparer is not null
            ? comparer.Equals(result.Value, expectedValue)
            : Equals(result.Value, expectedValue);

        return !success
            ? result
            : Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, ErrorArgument.FromObject(expectedValue)));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<T> NotEqual<T>(this Result<T> result, T expectedValue, IEqualityComparer<T> comparer, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        bool success = comparer is not null
            ? comparer.Equals(result.Value, expectedValue)
            : Equals(result.Value, expectedValue);

        if (success)
        {
            var propertyName = path.Format(member);
            return Result.Failure<T>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, ErrorArgument.FromObject(expectedValue)));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<string> NotEqual(this Result<string> result, string expectedValue, StringComparison stringComparisonType, string propertyName)
    {
        if (result.IsFailure) return result;

        return !result.Value.Equals(expectedValue, stringComparisonType)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<string> NotEqual(this Result<string> result, string expectedValue, StringComparison stringComparisonType,
        PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value.Equals(expectedValue, stringComparisonType))
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<byte> NotEqual(this Result<byte> result, byte expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<byte> NotEqual(this Result<byte> result, byte expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<short> NotEqual(this Result<short> result, short expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<short> NotEqual(this Result<short> result, short expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<int> NotEqual(this Result<int> result, int expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<int> NotEqual(this Result<int> result, int expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<ushort> NotEqual(this Result<ushort> result, ushort expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<ushort> NotEqual(this Result<ushort> result, ushort expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ushort>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<uint> NotEqual(this Result<uint> result, uint expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<uint> NotEqual(this Result<uint> result, uint expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<ulong> NotEqual(this Result<ulong> result, ulong expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<ulong> NotEqual(this Result<ulong> result, ulong expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<long> NotEqual(this Result<long> result, long expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<long> NotEqual(this Result<long> result, long expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<float> NotEqual(this Result<float> result, float expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<float> NotEqual(this Result<float> result, float expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<double> NotEqual(this Result<double> result, double expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<double> NotEqual(this Result<double> result, double expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<decimal> NotEqual(this Result<decimal> result, decimal expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<decimal> NotEqual(this Result<decimal> result, decimal expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<BigInteger> NotEqual(this Result<BigInteger> result, BigInteger expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<BigInteger> NotEqual(this Result<BigInteger> result, BigInteger expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<TimeSpan> NotEqual(this Result<TimeSpan> result, TimeSpan expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<TimeSpan> NotEqual(this Result<TimeSpan> result, TimeSpan expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<Guid> NotEqual(this Result<Guid> result, Guid expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<Guid> NotEqual(this Result<Guid> result, Guid expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<Guid>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<DateOnly> NotEqual(this Result<DateOnly> result, DateOnly expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<DateOnly> NotEqual(this Result<DateOnly> result, DateOnly expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<TimeOnly> NotEqual(this Result<TimeOnly> result, TimeOnly expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<TimeOnly> NotEqual(this Result<TimeOnly> result, TimeOnly expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<DateTime> NotEqual(this Result<DateTime> result, DateTime expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<DateTime> NotEqual(this Result<DateTime> result, DateTime expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> NotEqual(this Result<DateTimeOffset> result, DateTimeOffset expectedValue, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value != expectedValue
            ? result
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
    }

    /// <summary>
    ///     Ensure value is not equal to given value.
    /// </summary>
    public static Result<DateTimeOffset> NotEqual(this Result<DateTimeOffset> result, DateTimeOffset expectedValue, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        if (result.Value == expectedValue)
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, propertyName, expectedValue));
        }

        return result;
    }
}
