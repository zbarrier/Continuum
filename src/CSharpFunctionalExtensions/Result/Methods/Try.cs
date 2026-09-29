using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public partial struct Result
    {
        /// <summary>
        ///     Attempts to execute the supplied action. Returns a Result indicating whether the action executed successfully.
        /// </summary>
        public static Result Try(Action action, Func<Exception, Error>? errorHandler = null)
        {
            errorHandler ??= Configuration.DefaultTryErrorHandler;

            try
            {
                action();
                return Success();
            }
            catch (Exception exc)
            {
                Error message = errorHandler(exc);
                return Failure(message);
            }
        }

        /// <summary>
        ///     Attempts to execute the supplied function. Returns a Result indicating whether the function executed successfully.
        ///     If the function executed successfully, the result contains its return value.
        /// </summary>
        public static Result<T> Try<T>(Func<T> func, Func<Exception, Error>? errorHandler = null)
        {
            errorHandler ??= Configuration.DefaultTryErrorHandler;

            try
            {
                return Success(func());
            }
            catch (Exception exc)
            {
                Error message = errorHandler(exc);
                return Failure<T>(message);
            }
        }
    }
}
