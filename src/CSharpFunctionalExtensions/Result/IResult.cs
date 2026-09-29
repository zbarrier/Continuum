namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Represents the success or failure state of an operation.
/// </summary>
public interface IResult
{
    /// <summary>
    ///     Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }
    /// <summary>
    ///     Gets a value indicating whether the operation succeeded.
    /// </summary>
    bool IsSuccess { get; }
}

/// <summary>
///     Represents an object that exposes a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IValue<out T>
{
    /// <summary>
    ///     Gets the value.
    /// </summary>
    T Value { get; }
}

/// <summary>
///     Represents an object that exposes an <see cref="CSharpFunctionalExtensions.Error"/>.
/// </summary>
public interface IError
{
    /// <summary>
    ///     Gets the error.
    /// </summary>
    Error Error { get; }
}

/// <summary>
///     Represents the outcome of an operation that produces a value of type <typeparamref name="T"/> on success.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public interface IResult<out T> : IValue<T>, IResult, IError
{
}
