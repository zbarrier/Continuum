using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Diagnostics;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies how receiver statuses map to health results.
/// </summary>
public class KurrentDBQueueReceiversHealthCheckTests
{
    private static Task<HealthCheckResult> CheckAsync(KurrentDBQueueReceiverStatusRegistry registry)
    {
        var healthCheck = new KurrentDBQueueReceiversHealthCheck(registry);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, null)
        };
        return healthCheck.CheckHealthAsync(context);
    }

    [Fact]
    public async Task Running_Receivers_Are_Healthy()
    {
        // Arrange
        var registry = new KurrentDBQueueReceiverStatusRegistry();
        registry.Report(new KurrentDBQueueReceiverStatus("provider", "queue-0", KurrentDBQueueReceiverState.Running));

        // Act
        var result = await CheckAsync(registry);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Theory]
    [InlineData(KurrentDBQueueReceiverState.Starting)]
    [InlineData(KurrentDBQueueReceiverState.Reconnecting)]
    public async Task Starting_Or_Reconnecting_Receivers_Are_Degraded(KurrentDBQueueReceiverState state)
    {
        // Arrange
        var registry = new KurrentDBQueueReceiverStatusRegistry();
        registry.Report(new KurrentDBQueueReceiverStatus("provider", "queue-0", KurrentDBQueueReceiverState.Running));
        registry.Report(new KurrentDBQueueReceiverStatus("provider", "queue-1", state));

        // Act
        var result = await CheckAsync(registry);

        // Assert
        Assert.Equal(HealthStatus.Degraded, result.Status);
    }

    [Fact]
    public async Task A_Faulted_Receiver_Is_Unhealthy()
    {
        // Arrange
        var registry = new KurrentDBQueueReceiverStatusRegistry();
        var exception = new InvalidOperationException("KurrentDB is unavailable.");
        registry.Report(new KurrentDBQueueReceiverStatus("provider", "queue-0", KurrentDBQueueReceiverState.Reconnecting));
        registry.Report(new KurrentDBQueueReceiverStatus("provider", "queue-1", KurrentDBQueueReceiverState.Faulted, "failed", exception));

        // Act
        var result = await CheckAsync(registry);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Same(exception, result.Exception);
    }

    [Fact]
    public void Statuses_Are_Kept_Per_Provider_And_Queue()
    {
        // Arrange
        var registry = new KurrentDBQueueReceiverStatusRegistry();

        // Act
        registry.Report(new KurrentDBQueueReceiverStatus("provider-a", "queue-0", KurrentDBQueueReceiverState.Running));
        registry.Report(new KurrentDBQueueReceiverStatus("provider-b", "queue-0", KurrentDBQueueReceiverState.Faulted));
        registry.Remove("provider-a", "queue-0");

        // Assert
        var status = Assert.Single(registry.Statuses);
        Assert.Equal("provider-b", status.ProviderName);
    }
}
