using Orleans.Providers.Streams.KurrentDB;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that a halted event is reported once, reported again after its report expires, and tracked per event.
/// </summary>
public class HaltedEventReporterTests
{
    [Fact]
    public void Reports_An_Event_Only_Once_Within_The_Interval()
    {
        var clock = new ManualTimeProvider();
        using var reporter = new HaltedEventReporter(clock);

        Assert.True(reporter.ShouldReport("event-1"));
        Assert.False(reporter.ShouldReport("event-1"));

        clock.Advance(HaltedEventReporter.ReportInterval - TimeSpan.FromSeconds(1));
        Assert.False(reporter.ShouldReport("event-1"));
    }

    [Fact]
    public void Reports_An_Event_Again_After_The_Interval_Expires()
    {
        var clock = new ManualTimeProvider();
        using var reporter = new HaltedEventReporter(clock);

        Assert.True(reporter.ShouldReport("event-1"));

        clock.Advance(HaltedEventReporter.ReportInterval + TimeSpan.FromSeconds(1));
        Assert.True(reporter.ShouldReport("event-1"));
        Assert.False(reporter.ShouldReport("event-1"));
    }

    [Fact]
    public void Tracks_Each_Event_Independently()
    {
        using var reporter = new HaltedEventReporter(new ManualTimeProvider());

        Assert.True(reporter.ShouldReport("event-1"));
        Assert.True(reporter.ShouldReport("event-2"));
        Assert.False(reporter.ShouldReport("event-1"));
        Assert.False(reporter.ShouldReport("event-2"));
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
