using Microsoft.Extensions.Logging;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    public static class Configuration
    {
        public static bool DefaultConfigureAwait = false;

        public static Func<Exception, Error> DefaultTryErrorHandler =
            exc => RequestErrors.NewUnknown("{0}", exc.Message);

        public static Func<ILogger, Exception, Error> DefaultTryErrorHandlerWithLogging =
            (logger, exc) =>
            {
                logger.LogError(exc, "Exception caught! {ExceptionMessage}", exc.Message);
                return RequestErrors.NewUnknown("{0}", exc.Message);
            };
    }
}