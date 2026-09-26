using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public partial struct Result
    {
        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static async Task<Result> SuccessIf(Func<Task<bool>> predicate, Error error)
        {
            bool isSuccess = await predicate().DefaultAwait();
            return SuccessIf(isSuccess, error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static async Task<Result> SuccessIf(Func<Task<bool>> predicate, Func<Error> errorFactory)
        {
            bool isSuccess = await predicate().DefaultAwait();
            return SuccessIf(isSuccess, errorFactory);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static async Task<Result<T>> SuccessIf<T>(Func<Task<bool>> predicate, T value, Error error)
        {
            bool isSuccess = await predicate().DefaultAwait();
            return SuccessIf(isSuccess, value, error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static async Task<Result<T>> SuccessIf<T>(Func<Task<bool>> predicate, T value, Func<Error> errorFactory)
        {
            bool isSuccess = await predicate().DefaultAwait();
            return SuccessIf(isSuccess, value, errorFactory);
        }
    }
}
