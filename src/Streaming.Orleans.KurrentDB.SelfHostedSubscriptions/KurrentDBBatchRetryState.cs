using Microsoft.Extensions.Logging;

using Polly;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     Describes a batch being handed to a subscription and the failures processing it has had so far.
/// </summary>
/// <remarks>
///     The receiver attaches it to the <see cref="ResilienceContext" /> of each handler call so that retry logging
///     can identify the stuck batch and how long it has been stuck.
/// </remarks>
internal sealed class KurrentDBBatchRetryState
{
    /// <summary>The key the state is stored under in <see cref="ResilienceContext.Properties" />.</summary>
    public static readonly ResiliencePropertyKey<KurrentDBBatchRetryState> Key = new("Continuum.KurrentDB.BatchRetryState");

    private readonly TimeProvider _timeProvider;
    private long? _firstFailureTimestamp;

    public KurrentDBBatchRetryState(int eventCount, ulong firstPosition, ulong lastPosition, TimeProvider timeProvider)
    {
        EventCount = eventCount;
        FirstPosition = firstPosition;
        LastPosition = lastPosition;
        _timeProvider = timeProvider;
    }

    /// <summary>The number of events in the batch.</summary>
    public int EventCount { get; }

    /// <summary>The position of the first event in the batch.</summary>
    public ulong FirstPosition { get; }

    /// <summary>The position the batch is checkpointed at once processed.</summary>
    public ulong LastPosition { get; }

    /// <summary>The number of times processing the batch has failed.</summary>
    public int FailedAttempts { get; private set; }

    /// <summary>The time since processing the batch first failed, or zero when it has not failed.</summary>
    public TimeSpan Elapsed => _firstFailureTimestamp is { } start ? _timeProvider.GetElapsedTime(start) : TimeSpan.Zero;

    /// <summary>Records a failed attempt to process the batch.</summary>
    public void RecordFailure()
    {
        _firstFailureTimestamp ??= _timeProvider.GetTimestamp();
        FailedAttempts++;
    }

    /// <summary>
    ///     Gets the level a retry after <paramref name="failedAttempts" /> failures is logged at, or
    ///     <see langword="null" /> when it is not logged.
    /// </summary>
    /// <remarks>
    ///     The first five failures are warnings so brief blips are visible without alarming. The next five are errors
    ///     because the problem is now real. After that only every tenth failure is logged, roughly every five minutes
    ///     at a thirty second delay cap, so a long outage keeps a steady signal without flooding the logs.
    /// </remarks>
    public static LogLevel? GetRetryLogLevel(int failedAttempts) => failedAttempts switch
    {
        <= 5 => LogLevel.Warning,
        <= 10 => LogLevel.Error,
        _ when failedAttempts % 10 == 0 => LogLevel.Error,
        _ => null
    };
}

/// <summary>
///     Thrown when a batch could not be processed and the resilience pipeline gave up retrying it.
/// </summary>
internal sealed class KurrentDBBatchProcessingException : Exception
{
    public KurrentDBBatchProcessingException(KurrentDBBatchRetryState state, Exception innerException)
        : base($"Failed to process a batch of {state.EventCount} events from position {state.FirstPosition} to {state.LastPosition} after {state.FailedAttempts} attempts over {state.Elapsed}.", innerException)
    {
        State = state;
    }

    /// <summary>The batch and the failures processing it had.</summary>
    public KurrentDBBatchRetryState State { get; }
}
