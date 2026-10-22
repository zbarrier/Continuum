namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     Calculates the delay before a dropped catch-up subscription reconnects.
/// </summary>
internal static class KurrentDBReconnectBackoff
{
    /// <summary>
    ///     Calculates an exponential delay with equal jitter, capped at <paramref name="maxDelay" />.
    /// </summary>
    /// <param name="attempt">The 1-based number of consecutive reconnect attempts.</param>
    /// <param name="baseDelay">The delay before the first reconnect attempt.</param>
    /// <param name="maxDelay">The cap on the delay.</param>
    /// <param name="jitter">A value in [0, 1) used to spread the delay.</param>
    public static TimeSpan GetDelay(int attempt, TimeSpan baseDelay, TimeSpan maxDelay, double jitter)
    {
        var exponent = Math.Min(Math.Max(attempt - 1, 0), 30);
        var delay = Math.Min(baseDelay.TotalMilliseconds * Math.Pow(2, exponent), maxDelay.TotalMilliseconds);

        // Half fixed, half random, so the delay never collapses to zero.
        return TimeSpan.FromMilliseconds(delay / 2 + delay / 2 * jitter);
    }
}
