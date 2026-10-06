namespace Continuum.Streaming;

public interface IStreamSubscription
{
    public string SubscriptionName { get; }
}

public interface IHostedStreamSubscription : IStreamSubscription
{
    Task Start(CancellationToken cancellationToken);
    Task Stop(CancellationToken cancellationToken);
}
