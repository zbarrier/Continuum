using Microsoft.Extensions.Options;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.Configuration;

/// <summary>
///     Configuration validator for EventStoreDBLogConsistentStorageOptions.
/// </summary>
public class CosmosDBCatchupSubscriptionOptionsValidator : IConfigurationValidator
{
    private readonly CosmosDBCatchupSubscriptionOptions _options;
    private readonly CosmosDBCatchupSubscriptionInfo _subscriptionInfo;

    /// <summary>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="subscriptionInfo"></param>
    /// <exception cref="OrleansConfigurationException"></exception>
    public CosmosDBCatchupSubscriptionOptionsValidator(CosmosDBCatchupSubscriptionOptions options, 
        CosmosDBCatchupSubscriptionInfo subscriptionInfo)
    {
        _options = options ?? throw new OrleansConfigurationException($"Invalid CosmosDBCatchupSubscriptionOptions for CosmosDBCatchupSubscription {subscriptionInfo.Name}. Options is required.");
        _subscriptionInfo = subscriptionInfo;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.ConnectionName)} is required.");
        }
        if (string.IsNullOrWhiteSpace(_options.DatabaseName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.DatabaseName)} is required.");
        }
        if (string.IsNullOrWhiteSpace(_options.LeaseContainerName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.LeaseContainerName)} is required.");
        }
        if (string.IsNullOrWhiteSpace(_options.MonitoredContainerName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.MonitoredContainerName)} is required.");
        }
        if (string.IsNullOrWhiteSpace(_options.InstanceName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.InstanceName)} is required.");
        }
        if (_options.StartingPositionType == StartingPositionType.FromDateTime && _options.StartingPositionDateTime is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.StartingPositionDateTime)} is required when {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.StartingPositionType)} equals {StartingPositionType.FromDateTime}.");
        }
        if (_options.JsonSerializer is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.JsonSerializer)} is required.");
        }
        if (_options.StreamNameParser is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.StreamNameParser)} is required.");
        }
        if (_options.TypeMapper is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for CosmosDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(CosmosDBCatchupSubscriptionOptions)}.{nameof(_options.TypeMapper)} is required.");
        }
    }
}
