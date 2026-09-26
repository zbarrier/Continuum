using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result FailureIf(bool isFailure, Error error)
        => SuccessIf(!isFailure, error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result FailureIf(bool isFailure, Func<Error> errorFactory)
        => SuccessIf(!isFailure, errorFactory);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result FailureIf(Func<bool> failurePredicate, Error error)
        => SuccessIf(!failurePredicate(), error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result FailureIf(Func<bool> failurePredicate, Func<Error> errorFactory)
        => SuccessIf(!failurePredicate(), errorFactory);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<T> FailureIf<T>(bool isFailure, T value, Error error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<T> FailureIf<T>(bool isFailure, T value, Func<Error> errorFactory)
        => SuccessIf(!isFailure, value, errorFactory);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<T> FailureIf<T>(Func<bool> failurePredicate, in T value, Error error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<T> FailureIf<T>(Func<bool> failurePredicate, in T value, Func<Error> errorFactory)
        => SuccessIf(!failurePredicate(), value, errorFactory);
}
