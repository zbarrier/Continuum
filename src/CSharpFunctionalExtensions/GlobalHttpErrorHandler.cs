using Microsoft.Extensions.Logging;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Provides default mappings from HTTP client exceptions to <see cref="Error"/> instances.
/// </summary>
public static class GlobalHttpErrorHandler
{
    /// <summary>
    ///     Logs the exception and maps it to an <see cref="Error"/>: timeouts and cancellations become deadline-exceeded errors; all other exceptions become unknown errors.
    /// </summary>
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
