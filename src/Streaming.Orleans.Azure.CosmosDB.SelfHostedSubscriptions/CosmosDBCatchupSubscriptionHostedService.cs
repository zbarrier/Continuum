using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions;

/// <summary>Starts and stops all registered hosted stream subscriptions with the host.</summary>
public sealed class CosmosDBCatchupSubscriptionHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CosmosDBCatchupSubscriptionHostedService> _logger;
    private readonly IEnumerable<IHostedStreamSubscription> _subscriptions;

    /// <summary>Creates the hosted service.</summary>
    public CosmosDBCatchupSubscriptionHostedService(IServiceProvider serviceProvider, 
        ILogger<CosmosDBCatchupSubscriptionHostedService> logger, 
        IEnumerable<IHostedStreamSubscription> subscriptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _subscriptions = subscriptions;
    }

    /// <summary>Starts all subscriptions.</summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("CosmosDB subscriptions STARTING.");

        var tasks = new List<Task>(_subscriptions.Count());
        foreach (var subscription in _subscriptions)
        {
            tasks.Add(subscription.StartAsync(cancellationToken));
        }
        await Task.WhenAll(tasks);

        _logger.LogWarning("CosmosDB subscriptions STARTED.");
    }

    /// <summary>Stops all subscriptions.</summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("CosmosDB subscriptions STOPPING.");

        var tasks = new List<Task>(_subscriptions.Count());
        foreach (var subscription in _subscriptions)
        {
            tasks.Add(subscription.StopAsync(cancellationToken));
        }
        await Task.WhenAll(tasks);

        _logger.LogWarning("CosmosDB subscriptions STOPPED.");
    }
}
