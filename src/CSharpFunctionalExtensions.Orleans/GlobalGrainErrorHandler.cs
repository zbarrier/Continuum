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
                logger.GrainCallTimedOut(ex);
                error = RequestErrors.NewDeadlineExceeded("The operation timed out before it could complete.");
                break;
            default:
                logger.GrainCallFailed(ex);
                error = RequestErrors.NewUnknown("An unexpected error occurred.");
                break;
        }
        return error;
    };
}

internal static partial class GlobalGrainErrorHandlerLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Grain call failed with timeout exception.")]
    public static partial void GrainCallTimedOut(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Grain call failed with exception.")]
    public static partial void GrainCallFailed(this ILogger logger, Exception exception);
}
