namespace Continuum.Streaming;

public interface IStreamSubscription
{
    string SubscriptionName { get; }
}

public interface IHostedStreamSubscription : IStreamSubscription
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
