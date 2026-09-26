using Microsoft.Extensions.Logging;

namespace Continuum.CSharpFunctionalExtensions;

public static class GlobalHttpErrorHandler
{
    public static readonly Func<Exception, ILogger, Error> Default = (ex, logger) =>
    {
        Error error;
        switch (ex)
        {
            case TimeoutException:
            case TaskCanceledException:
                logger.LogError(ex, "Http request timed out.");
                error = RequestErrors.NewDeadlineExceeded("The operation timed out before it could complete.");
                break;

            case HttpRequestException:
            default:
                logger.LogError(ex, "Http request failed with an exception.");
                error = RequestErrors.NewUnknown("An unexpected request error occurred.");
                break;
        }
        return error;
    };
}
