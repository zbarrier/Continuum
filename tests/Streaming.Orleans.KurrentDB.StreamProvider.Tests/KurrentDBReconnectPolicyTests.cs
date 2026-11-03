using Microsoft.Extensions.Logging;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies the reconnect backoff doubles from one second up to a thirty second ceiling, never collapses to zero,
///     and escalates its log level without flooding the log during a long outage.
/// </summary>
public class KurrentDBReconnectPolicyTests
{
    [Theory]
    [InlineData(1, 0.0, 500)]
    [InlineData(1, 1.0, 1000)]
    [InlineData(2, 0.0, 1000)]
    [InlineData(3, 0.0, 2000)]
    [InlineData(5, 1.0, 16000)]
    public void Doubles_the_delay_with_each_attempt(int attempt, double jitter, double expectedMilliseconds)
    {
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), KurrentDBReconnectPolicy.GetDelay(attempt, jitter));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Treats_a_non_positive_attempt_as_the_first(int attempt)
    {
        Assert.Equal(KurrentDBReconnectPolicy.GetDelay(1, 0.5), KurrentDBReconnectPolicy.GetDelay(attempt, 0.5));
    }

    [Theory]
    [InlineData(6)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(int.MaxValue)]
    public void Caps_the_delay_at_thirty_seconds(int attempt)
    {
        Assert.Equal(TimeSpan.FromSeconds(30), KurrentDBReconnectPolicy.GetDelay(attempt, 1.0));
        Assert.Equal(TimeSpan.FromSeconds(15), KurrentDBReconnectPolicy.GetDelay(attempt, 0.0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(100)]
    public void Keeps_half_the_delay_fixed_so_jitter_never_reaches_zero(int attempt)
    {
        var minimum = KurrentDBReconnectPolicy.GetDelay(attempt, 0.0);
        var maximum = KurrentDBReconnectPolicy.GetDelay(attempt, 1.0);

        Assert.True(minimum > TimeSpan.Zero);
        Assert.Equal(maximum, minimum * 2);
        var middle = KurrentDBReconnectPolicy.GetDelay(attempt, 0.5);
        Assert.InRange(middle, minimum, maximum);
    }

    [Theory]
    [InlineData(1, LogLevel.Warning)]
    [InlineData(5, LogLevel.Warning)]
    [InlineData(6, LogLevel.Error)]
    [InlineData(10, LogLevel.Error)]
    [InlineData(11, null)]
    [InlineData(19, null)]
    [InlineData(20, LogLevel.Error)]
    [InlineData(21, null)]
    [InlineData(100, LogLevel.Error)]
    public void Escalates_and_then_throttles_the_log_level(int failedAttempts, LogLevel? expected)
    {
        Assert.Equal(expected, KurrentDBReconnectPolicy.GetLogLevel(failedAttempts));
    }
}
