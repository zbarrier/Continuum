using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static Result TapTry(this Result result, Action action, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess)
                    action();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return Result.Failure(error);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static Result<T> TapTry<T>(this Result<T> result, Action action, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess)
                    action();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static Result<T> TapTry<T>(this Result<T> result, Action<T> action, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess)
                    action(result.Value);

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }
    }
}
