using Microsoft.Extensions.Logging;

namespace Continuum.CSharpFunctionalExtensions.Orleans;

/// <summary>
///     Maps exceptions thrown by grain calls to <see cref="Error"/> values.
/// </summary>
public static class GlobalGrainErrorHandler
{
    /// <summary>
    ///     Logs the exception and returns a <see cref="RequestError"/>: DeadlineExceeded for <see cref="TimeoutException"/>,
    ///     otherwise Unknown.
    /// </summary>
    public static readonly Func<Exception, ILogger, Error> Default = (ex, logger) =>
    {
        Error error;
        switch (ex)
        {
            case System.TimeoutException:
                logger.LogError(ex, "Grain call failed with timeout exception.");
                error = RequestErrors.NewDeadlineExceeded("The operation timed out before it could complete.");
                break;
            default:
                logger.LogError(ex, "Grain call failed with exception.");
                error = RequestErrors.NewUnknown("An unexpected error occurred.");
                break;
        }
        return error;
    };
}
