using Microsoft.Extensions.Caching.Memory;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Decides whether a halted event should be reported, so the error is logged once per event rather than on every
///     Orleans delivery retry.
/// </summary>
/// <remarks>
///     Entries expire after <see cref="ReportInterval" />, so an event that stays halted is reported again
///     periodically and memory does not grow on a long running silo. The cache is also capped at
///     <see cref="MaxTrackedEvents" /> entries; past that, new events are reported without being tracked.
/// </remarks>
internal sealed class HaltedEventReporter : IDisposable
{
    internal static readonly TimeSpan ReportInterval = TimeSpan.FromMinutes(5);
    internal const int MaxTrackedEvents = 10_000;

    private readonly MemoryCache _reported;

    public HaltedEventReporter(TimeProvider? timeProvider = null)
    {
        var options = new MemoryCacheOptions { SizeLimit = MaxTrackedEvents };
        if (timeProvider is not null)
        {
            options.Clock = new TimeProviderClock(timeProvider);
        }
        _reported = new MemoryCache(options);
    }

    /// <summary>
    ///     Returns <see langword="true" /> the first time an event is seen, and again once its report has expired.
    /// </summary>
    public bool ShouldReport(string eventId)
    {
        if (_reported.TryGetValue(eventId, out _))
        {
            return false;
        }
        _reported.Set(eventId, true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ReportInterval,
            Size = 1,
        });
        return true;
    }

    public void Dispose() => _reported.Dispose();

#pragma warning disable CS0618 // ISystemClock is the only clock abstraction MemoryCacheOptions accepts.
    private sealed class TimeProviderClock(TimeProvider timeProvider) : Microsoft.Extensions.Internal.ISystemClock
    {
        public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
    }
#pragma warning restore CS0618
}
