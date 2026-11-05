namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Diagnostics;

/// <summary>
///     Reports the status and metrics of one queue receiver.
/// </summary>
public sealed class KurrentDBQueueReceiverDiagnostics
{
    private readonly KurrentDBQueueReceiverStatusRegistry _statusRegistry;
    private readonly KurrentDBStreamProviderMetrics _metrics;
    private readonly string _providerName;
    private readonly string _queueName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueReceiverDiagnostics" /> class.
    /// </summary>
    public KurrentDBQueueReceiverDiagnostics(KurrentDBQueueReceiverStatusRegistry statusRegistry, KurrentDBStreamProviderMetrics metrics,
        string providerName, string queueName)
    {
        ArgumentNullException.ThrowIfNull(statusRegistry);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(queueName);
        _statusRegistry = statusRegistry;
        _metrics = metrics;
        _providerName = providerName;
        _queueName = queueName;
    }

    internal void Starting() => Report(KurrentDBQueueReceiverState.Starting);

    internal void Running() => Report(KurrentDBQueueReceiverState.Running);

    internal void InitializationFailed(Exception exception)
    {
        _metrics.InitializationFailed(_providerName, _queueName);
        Report(KurrentDBQueueReceiverState.Faulted, "Initialization failed. It is retried on the next read.", exception);
    }

    internal void Reconnecting(Exception? exception) =>
        Report(KurrentDBQueueReceiverState.Reconnecting, "The subscription dropped and is reconnecting.", exception);

    internal void Reconnected()
    {
        _metrics.Reconnected(_providerName, _queueName);
        Running();
    }

    internal void Stopped() => _statusRegistry.Remove(_providerName, _queueName);

    private void Report(KurrentDBQueueReceiverState state, string? description = null, Exception? exception = null) =>
        _statusRegistry.Report(new KurrentDBQueueReceiverStatus(_providerName, _queueName, state, description, exception));
}
