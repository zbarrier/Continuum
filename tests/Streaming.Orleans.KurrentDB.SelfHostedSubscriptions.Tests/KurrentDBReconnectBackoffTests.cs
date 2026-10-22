namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that reconnect delays grow exponentially, stay within the cap and never collapse to zero.
/// </summary>
public class KurrentDBReconnectBackoffTests
{
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    [InlineData(5, 16)]
    [InlineData(6, 30)]
    [InlineData(1000, 30)]
    public void GetDelay_WithMaximumJitter_GrowsExponentiallyUpToCap(int attempt, int expectedSeconds)
    {
        var delay = KurrentDBReconnectBackoff.GetDelay(attempt, BaseDelay, MaxDelay, 1.0);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), delay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(int.MaxValue)]
    public void GetDelay_WithNoJitter_IsHalfTheExponentialDelay(int attempt)
    {
        var full = KurrentDBReconnectBackoff.GetDelay(attempt, BaseDelay, MaxDelay, 1.0);
        var delay = KurrentDBReconnectBackoff.GetDelay(attempt, BaseDelay, MaxDelay, 0.0);

        Assert.Equal(full / 2, delay);
        Assert.True(delay > TimeSpan.Zero);
    }
}
