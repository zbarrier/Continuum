using Continuum.Serialization.Orleans;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

namespace Continuum.Streaming.Orleans.KurrentDB.Configuration;

/// <summary>
///     Configuration validator for KurrentDBChannelCatchupSubscriptionOptions.
/// </summary>
public class KurrentDBCatchupSubscriptionOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBCatchupSubscriptionOptions _options;
    private readonly KurrentDBCatchupSubscriptionInfo _subscriptionInfo;

    /// <summary>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="subscriptionInfo"></param>
    /// <exception cref="OrleansConfigurationException"></exception>
    public KurrentDBCatchupSubscriptionOptionsValidator(KurrentDBCatchupSubscriptionOptions options, 
        KurrentDBCatchupSubscriptionInfo subscriptionInfo)
    {
        _options = options ?? throw new OrleansConfigurationException($"Invalid {nameof(KurrentDBCatchupSubscriptionOptions)} for KurrentDBChannelCatchupSubscription {subscriptionInfo.Name}. Options is required.");
        _subscriptionInfo = subscriptionInfo;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.ConnectionName)} is required.");
        }
        if (_options.BufferOptions is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.BufferOptions)} is required.");
        }
        if (_options.BufferOptions.MaxEventsPerProcessLoop is < 1 or > 100_000)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBChannelBufferOptions)}.{nameof(KurrentDBChannelBufferOptions.MaxEventsPerProcessLoop)} must be between 1 and 100,000.");
        }
        if (_options.BufferOptions.MaxUnprocessedEventsBeforeStop is < 1 or > 1_000_000)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBChannelBufferOptions)}.{nameof(KurrentDBChannelBufferOptions.MaxUnprocessedEventsBeforeStop)} must be between 1 and 1,000,000.");
        }
        if (_options.CheckpointMonitorOptions is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.CheckpointMonitorOptions)} is required.");
        }
        if (_options.CheckpointMonitorOptions.KurrentDBCheckpointInterval is < 1 or > 1_000_000)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCheckpointMonitorOptions)}.{nameof(KurrentDBCheckpointMonitorOptions.KurrentDBCheckpointInterval)} must be between 1 and 1,000,000.");
        }
        if (_options.CheckpointMonitorOptions.MaxEventsBeforeCommit is < 1 or > 1_000_000)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCheckpointMonitorOptions)}.{nameof(KurrentDBCheckpointMonitorOptions.MaxEventsBeforeCommit)} must be between 1 and 1,000,000.");
        }
        if (_options.CheckpointMonitorOptions.MaxTimeBeforeCommit < TimeSpan.FromSeconds(1) || _options.CheckpointMonitorOptions.MaxTimeBeforeCommit > TimeSpan.FromDays(1))
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCheckpointMonitorOptions)}.{nameof(KurrentDBCheckpointMonitorOptions.MaxTimeBeforeCommit)} must be between 00:00:01 and 1.00:00:00.");
        }
        if (_options.Credentials is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.Credentials)} is required.");
        }
        if (!_options.Credentials.UseDefault)
        {
            bool isValid = !string.IsNullOrWhiteSpace(_options.Credentials.AuthToken) ||
                (!string.IsNullOrWhiteSpace(_options.Credentials.Username) && !string.IsNullOrWhiteSpace(_options.Credentials.Password));
            if (!isValid)
            {
                throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBCatchupSubscriptionOptions)} with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.Credentials)} requires an AuthToken or Username and Password when not using the connection default.");
            }
        }
        if (_options.CheckpointStore is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.CheckpointStore)} is required.");
        }
        if (_options.GrainStorageSerializer is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.GrainStorageSerializer)} is required.");
        }
        if (_options.TypeMapper is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for KurrentDB catch-up subscription with name {_subscriptionInfo.Name}. {nameof(KurrentDBCatchupSubscriptionOptions)}.{nameof(_options.TypeMapper)} is required.");
        }
    }
}
