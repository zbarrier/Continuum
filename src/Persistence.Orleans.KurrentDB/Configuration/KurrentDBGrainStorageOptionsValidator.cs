namespace Orleans.Configuration;

/// <summary>
///     Configuration validator for KurrentDBGrainStorageOptions.
/// </summary>
public class KurrentDBGrainStorageOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBGrainStorageOptions _options;
    private readonly string _name;

    /// <summary>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <exception cref="OrleansConfigurationException"></exception>
    public KurrentDBGrainStorageOptionsValidator(KurrentDBGrainStorageOptions options, string name)
    {
        _options = options ?? throw new OrleansConfigurationException($"Invalid KurrentDBGrainStorageOptions for KurrentDBGrainStorage {name}. Options is required.");
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.ConnectionName)} is required.");
        }
        if (_options.Credentials is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.Credentials)} is required.");
        }
        if (!_options.Credentials.IsValid)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.Credentials)} requires an AuthToken or Username and Password when not using the connection default.");
        }
        if (_options.MaxStateEventCount is < 1)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.MaxStateEventCount)} must be at least 1 or null.");
        }
        if (_options.GrainStorageSerializer is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.GrainStorageSerializer)} is required.");
        }
        if (_options.StreamNameFormatter is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.StreamNameFormatter)} is required.");
        }
        if (_options.EventIdGenerator is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.EventIdGenerator)} is required.");
        }
        if (_options.TypeMapper is null)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBGrainStorageOptions)} with name {_name}. {nameof(KurrentDBGrainStorageOptions)}.{nameof(_options.TypeMapper)} is required.");
        }
    }
}
