using Continuum.EventSourcing.Orleans.KurrentDB;

namespace Orleans.Configuration;

/// <summary>
///     Configuration validator for KurrentDBLogConsistentStorageOptions.
/// </summary>
public class KurrentDBLogConsistentStorageOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBLogConsistentStorageOptions _options;
    private readonly string _name;

    /// <summary>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <exception cref="OrleansConfigurationException"></exception>
    public KurrentDBLogConsistentStorageOptionsValidator(KurrentDBLogConsistentStorageOptions options, string name)
    {
        _options = options ?? throw new OrleansConfigurationException($"Invalid KurrentDBLogConsistentStorageOptions for KurrentDBLogConsistentStorage {name}. Options is required.");
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.ConnectionName)} is required.");
        }
        if (_options.Credentials is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.Credentials)} is required.");
        }
        if (!_options.Credentials.UseDefault)
        {
            bool isValid = !string.IsNullOrWhiteSpace(_options.Credentials.AuthToken) ||
                (!string.IsNullOrWhiteSpace(_options.Credentials.Username) && !string.IsNullOrWhiteSpace(_options.Credentials.Password));
            if (!isValid)
            {
                throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.Credentials)} requires an AuthToken or Username and Password when not using the connection default.");
            }
        }
        if (_options.GrainStorageSerializer is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.GrainStorageSerializer)} is required.");
        }
        if (_options.StreamNameFormatter is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.StreamNameFormatter)} is required.");
        }
        if (_options.EventIdGenerator is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.EventIdGenerator)} is required.");
        }
        if (_options.TypeMapper is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. {nameof(KurrentDBLogConsistentStorageOptions)}.{nameof(_options.TypeMapper)} is required.");
        }
    }
}
