using Microsoft.Extensions.Logging;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Global defaults used by result extension methods.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        ///     The <c>continueOnCapturedContext</c> value used when awaiting inside async extensions. Defaults to <see langword="false"/>.
        /// </summary>
        public static bool DefaultConfigureAwait = false;

        /// <summary>
        ///     Maps an exception caught by <c>Try</c> to an <see cref="Error"/> when no handler is supplied. Defaults to an unknown error carrying the exception message.
        /// </summary>
        public static Func<Exception, Error> DefaultTryErrorHandler =
            exc => RequestErrors.NewUnknown("{0}", exc.Message);

        /// <summary>
        ///     Logs an exception caught by <c>Try</c> and maps it to an <see cref="Error"/>. Defaults to an unknown error carrying the exception message.
        /// </summary>
        public static Func<ILogger, Exception, Error> DefaultTryErrorHandlerWithLogging =
            (logger, exc) =>
            {
                ResultLog.ExceptionCaught(logger, exc, exc.Message);
                return RequestErrors.NewUnknown("{0}", exc.Message);
            };
    }
}

internal static partial class ResultLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Exception caught! {ExceptionMessage}")]
    public static partial void ExceptionCaught(ILogger logger, Exception exception, string exceptionMessage);
}
