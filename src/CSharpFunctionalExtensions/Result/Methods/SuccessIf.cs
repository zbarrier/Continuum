using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public partial struct Result
    {
        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result SuccessIf(bool isSuccess, Error error)
        {
            return isSuccess
                ? Success()
                : Failure(error);
        }

        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result SuccessIf(bool isSuccess, Func<Error> errorFactory)
        {
            return isSuccess
                ? Success()
                : Failure(errorFactory());
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result SuccessIf(Func<bool> predicate, Error error)
        {
            return SuccessIf(predicate(), error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result SuccessIf(Func<bool> predicate, Func<Error> errorFactory)
        {
            return SuccessIf(predicate(), errorFactory);
        }

        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result<T> SuccessIf<T>(bool isSuccess, in T value, Error error)
        {
            return isSuccess
                ? Success(value)
                : Failure<T>(error);
        }

        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result<T> SuccessIf<T>(bool isSuccess, in T value, Func<Error> errorFactory)
        {
            return isSuccess
                ? Success(value)
                : Failure<T>(errorFactory());
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result<T> SuccessIf<T>(Func<bool> predicate, in T value, Error error)
        {
            return SuccessIf(predicate(), value, error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result<T> SuccessIf<T>(Func<bool> predicate, in T value, Func<Error> errorFactory)
        {
            return SuccessIf(predicate(), value, errorFactory);
        }
    }
}
