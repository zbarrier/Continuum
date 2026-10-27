using Microsoft.Extensions.Logging;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     The reconnect delay and log level policy shared by the KurrentDB receivers.
/// </summary>
internal static class KurrentDBReconnectPolicy
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Calculates an exponential delay with equal jitter, capped at thirty seconds.
    /// </summary>
    /// <param name="attempt">The 1-based number of consecutive reconnect attempts.</param>
    /// <param name="jitter">A value in [0, 1) used to spread the delay.</param>
    public static TimeSpan GetDelay(int attempt, double jitter)
    {
        var exponent = Math.Min(Math.Max(attempt - 1, 0), 30);
        var delay = Math.Min(InitialDelay.TotalMilliseconds * Math.Pow(2, exponent), MaxDelay.TotalMilliseconds);

        // Half fixed, half random, so the delay never collapses to zero.
        return TimeSpan.FromMilliseconds(delay / 2 + delay / 2 * jitter);
    }

    /// <summary>
    ///     The first five failures are warnings, the next five are errors, and after that only every tenth failure is
    ///     logged, so a long outage keeps a steady signal without flooding the logs.
    /// </summary>
    public static LogLevel? GetLogLevel(int failedAttempts) => failedAttempts switch
    {
        <= 5 => LogLevel.Warning,
        <= 10 => LogLevel.Error,
        _ when failedAttempts % 10 == 0 => LogLevel.Error,
        _ => null
    };
}
