using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Providers.Streams.Common;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies when each cache pressure monitor starts and stops throttling ingestion.
/// </summary>
public class CachePressureMonitorTests
{
    private static readonly DateTime Start = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private sealed class FakeMonitor(bool underPressure) : ICachePressureMonitor
    {
        public List<double> Contributions { get; } = [];

        public int Checks { get; private set; }

        public bool UnderPressure { get; set; } = underPressure;

        public ICacheMonitor CacheMonitor { set { } }

        public void RecordCachePressureContribution(double cachePressureContribution) => Contributions.Add(cachePressureContribution);

        public bool IsUnderPressure(DateTime utcNow)
        {
            Checks++;
            return UnderPressure;
        }
    }

    #region SlowConsumingPressureMonitor

    private static SlowConsumingPressureMonitor CreateSlowConsuming() =>
        new(0.5, TimeSpan.FromMinutes(1), NullLogger.Instance);

    [Fact]
    public void SlowConsuming_Is_not_under_pressure_without_contributions()
    {
        Assert.False(CreateSlowConsuming().IsUnderPressure(Start));
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    public void SlowConsuming_Ignores_contributions_at_or_below_the_threshold(double contribution)
    {
        var monitor = CreateSlowConsuming();
        monitor.RecordCachePressureContribution(contribution);

        Assert.False(monitor.IsUnderPressure(Start));
    }

    [Fact]
    public void SlowConsuming_Reacts_to_the_biggest_contribution_in_the_window()
    {
        var monitor = CreateSlowConsuming();
        monitor.RecordCachePressureContribution(0.1);
        monitor.RecordCachePressureContribution(0.9);
        monitor.RecordCachePressureContribution(0.2);

        Assert.True(monitor.IsUnderPressure(Start));
    }

    [Fact]
    public void SlowConsuming_Stays_under_pressure_for_the_whole_window()
    {
        var monitor = CreateSlowConsuming();
        monitor.RecordCachePressureContribution(0.9);
        Assert.True(monitor.IsUnderPressure(Start));

        monitor.RecordCachePressureContribution(0.1);

        Assert.True(monitor.IsUnderPressure(Start + TimeSpan.FromSeconds(30)));
        Assert.True(monitor.IsUnderPressure(Start + TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void SlowConsuming_Clears_pressure_after_a_healthy_window()
    {
        var monitor = CreateSlowConsuming();
        monitor.RecordCachePressureContribution(0.9);
        Assert.True(monitor.IsUnderPressure(Start));

        monitor.RecordCachePressureContribution(0.1);

        Assert.False(monitor.IsUnderPressure(Start + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1)));
    }

    #endregion

    #region AveragingCachePressureMonitor

    private static AveragingCachePressureMonitor CreateAveraging() => new(0.5, NullLogger.Instance);

    [Fact]
    public void Averaging_Is_not_under_pressure_without_contributions()
    {
        Assert.False(CreateAveraging().IsUnderPressure(Start));
    }

    [Fact]
    public void Averaging_Is_under_pressure_when_the_average_exceeds_the_threshold()
    {
        var monitor = CreateAveraging();
        monitor.RecordCachePressureContribution(0.6);
        monitor.RecordCachePressureContribution(0.8);

        Assert.True(monitor.IsUnderPressure(Start));
    }

    [Fact]
    public void Averaging_Is_not_under_pressure_when_the_average_is_at_or_below_the_threshold()
    {
        var monitor = CreateAveraging();
        monitor.RecordCachePressureContribution(0.1);
        monitor.RecordCachePressureContribution(0.3);

        Assert.False(monitor.IsUnderPressure(Start));
    }

    [Fact]
    public void Averaging_Weights_unhealthy_contributions_three_times_as_much()
    {
        var monitor = CreateAveraging();
        // Unweighted the average is 0.37, which is healthy. Weighted it is (0.9 * 3 + 0.1 + 0.1) / 5 = 0.58.
        monitor.RecordCachePressureContribution(0.9);
        monitor.RecordCachePressureContribution(0.1);
        monitor.RecordCachePressureContribution(0.1);

        Assert.True(monitor.IsUnderPressure(Start));
    }

    [Fact]
    public void Averaging_Only_recalculates_once_per_check_period()
    {
        var monitor = CreateAveraging();
        monitor.RecordCachePressureContribution(0.9);
        Assert.True(monitor.IsUnderPressure(Start));

        monitor.RecordCachePressureContribution(0.1);

        Assert.True(monitor.IsUnderPressure(Start + TimeSpan.FromSeconds(1)));
        Assert.False(monitor.IsUnderPressure(Start + TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public void Averaging_Keeps_its_state_when_a_period_has_no_contributions()
    {
        var monitor = CreateAveraging();
        monitor.RecordCachePressureContribution(0.9);
        Assert.True(monitor.IsUnderPressure(Start));

        Assert.True(monitor.IsUnderPressure(Start + TimeSpan.FromSeconds(3)));
    }

    #endregion

    #region AggregatedCachePressureMonitor

    [Fact]
    public void Aggregated_Is_not_under_pressure_without_monitors()
    {
        Assert.False(new AggregatedCachePressureMonitor(NullLogger.Instance).IsUnderPressure(Start));
    }

    [Fact]
    public void Aggregated_Forwards_every_contribution_to_every_monitor()
    {
        var first = new FakeMonitor(false);
        var second = new FakeMonitor(false);
        var aggregated = new AggregatedCachePressureMonitor(NullLogger.Instance);
        aggregated.AddCachePressureMonitor(first);
        aggregated.AddCachePressureMonitor(second);

        aggregated.RecordCachePressureContribution(0.3);
        aggregated.RecordCachePressureContribution(0.7);

        Assert.Equal([0.3, 0.7], first.Contributions);
        Assert.Equal([0.3, 0.7], second.Contributions);
    }

    [Fact]
    public void Aggregated_Is_under_pressure_when_any_monitor_is()
    {
        var healthy = new FakeMonitor(false);
        var pressured = new FakeMonitor(true);
        var aggregated = new AggregatedCachePressureMonitor(NullLogger.Instance);
        aggregated.AddCachePressureMonitor(healthy);
        aggregated.AddCachePressureMonitor(pressured);

        Assert.True(aggregated.IsUnderPressure(Start));
    }

    [Fact]
    public void Aggregated_Is_healthy_when_every_monitor_is()
    {
        var aggregated = new AggregatedCachePressureMonitor(NullLogger.Instance);
        aggregated.AddCachePressureMonitor(new FakeMonitor(false));
        aggregated.AddCachePressureMonitor(new FakeMonitor(false));

        Assert.False(aggregated.IsUnderPressure(Start));
    }

    [Fact]
    public void Aggregated_Stops_checking_after_the_first_monitor_under_pressure()
    {
        var pressured = new FakeMonitor(true);
        var unchecked_ = new FakeMonitor(false);
        var aggregated = new AggregatedCachePressureMonitor(NullLogger.Instance);
        aggregated.AddCachePressureMonitor(pressured);
        aggregated.AddCachePressureMonitor(unchecked_);

        Assert.True(aggregated.IsUnderPressure(Start));
        Assert.Equal(0, unchecked_.Checks);
    }

    [Fact]
    public void Aggregated_Follows_its_monitors_back_to_healthy()
    {
        var monitor = new FakeMonitor(true);
        var aggregated = new AggregatedCachePressureMonitor(NullLogger.Instance);
        aggregated.AddCachePressureMonitor(monitor);
        Assert.True(aggregated.IsUnderPressure(Start));

        monitor.UnderPressure = false;

        Assert.False(aggregated.IsUnderPressure(Start));
    }

    #endregion
}
