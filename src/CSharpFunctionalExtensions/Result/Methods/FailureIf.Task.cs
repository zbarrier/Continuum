using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public partial struct Result
    {
        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
        /// </summary>
        public static async Task<Result> FailureIf(Func<Task<bool>> failurePredicate, Error error)
        {
            bool isFailure = await failurePredicate().DefaultAwait();
            return SuccessIf(!isFailure, error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
        /// </summary>
        public static async Task<Result> FailureIf(Func<Task<bool>> failurePredicate, Func<Error> errorFactory)
        {
            bool isFailure = await failurePredicate().DefaultAwait();
            return SuccessIf(!isFailure, errorFactory);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
        /// </summary>
        public static async Task<Result<T>> FailureIf<T>(Func<Task<bool>> failurePredicate, T value, Error error)
        {
            bool isFailure = await failurePredicate().DefaultAwait();
            return SuccessIf(!isFailure, value, error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
        /// </summary>
        public static async Task<Result<T>> FailureIf<T>(Func<Task<bool>> failurePredicate, T value, Func<Error> errorFactory)
        {
            bool isFailure = await failurePredicate().DefaultAwait();
            return SuccessIf(!isFailure, value, errorFactory);
        }
    }
}
