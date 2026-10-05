using Continuum.EventSourcing.Orleans.CosmosDB;

using Orleans.Runtime;

namespace Orleans.Configuration;

/// <summary>
///     Configuration validator for CosmosDBLogConsistentStorageOptions.
/// </summary>
public class CosmosDBLogConsistentStorageOptionsValidator : IConfigurationValidator
{
    private readonly CosmosDBLogConsistentStorageOptions _options;
    private readonly string _name;

    /// <summary>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <exception cref="OrleansConfigurationException"></exception>
    public CosmosDBLogConsistentStorageOptionsValidator(CosmosDBLogConsistentStorageOptions options, string name)
    {
        _options = options ?? throw new OrleansConfigurationException($"Invalid CosmosDBLogConsistentStorageOptions for CosmosDBLogConsistentStorage {name}. Options is required.");
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.ConnectionName)} is required.");
        }
        if (string.IsNullOrWhiteSpace(_options.DatabaseName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.DatabaseName)} is required.");
        }
        if (string.IsNullOrWhiteSpace(_options.ContainerName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.ContainerName)} is required.");
        }
        if (_options.BatchSize is < 2 or > 100)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.BatchSize)} must be between 2 and 100.");
        }
        if (_options.QueryMaxItemCount < 1)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.QueryMaxItemCount)} must be greater than 0.");
        }
        if (_options.StartupConnectionTimeout <= TimeSpan.Zero)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.StartupConnectionTimeout)} must be greater than zero.");
        }
        if (!(_options.RequestChargeWarningThreshold >= 0))
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.RequestChargeWarningThreshold)} must not be negative.");
        }
        if (_options.GrainStorageSerializer is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.GrainStorageSerializer)} is required.");
        }
        if (_options.StreamNameFormatter is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.StreamNameFormatter)} is required.");
        }
        if (_options.EventIdGenerator is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.EventIdGenerator)} is required.");
        }
        if (_options.TypeMapper is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(CosmosDBLogConsistentStorage)} with name {_name}. {nameof(CosmosDBLogConsistentStorageOptions)}.{nameof(_options.TypeMapper)} is required.");
        }
    }
}
