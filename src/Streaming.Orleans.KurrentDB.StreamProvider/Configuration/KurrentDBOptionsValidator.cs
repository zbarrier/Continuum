using Orleans.Runtime;

namespace Orleans.Configuration;

/// <summary>
///  </summary>
public class KurrentDBOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBOptions _options;
    private readonly string _name;

    /// <summary>
    ///     Constructor for <see cref="KurrentDBOptionsValidator" />
    /// </summary>
    /// <param name="options">The <see cref="KurrentDBOptions" /> to validate.</param>
    /// <param name="name">The name of the stream provider.</param>
    public KurrentDBOptionsValidator(KurrentDBOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <summary>
    ///     Validates the KurrentDB configuration.
    /// </summary>
    /// <exception cref="OrleansConfigurationException">Thrown when <see cref="KurrentDBOptions.ConnectionName" /> or <see cref="KurrentDBOptions.Credentials" /> are not configured, or when <see cref="KurrentDBOptions.Name" /> is not set, or when <see cref="KurrentDBOptions.Queues" /> is empty.</exception>
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBOptions.ConnectionName)} is required so the keyed KurrentDB client, serializer and type mapper can be resolved.");
        }
        if (_options.Credentials is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBOptions.Credentials)} should not be null.");
        }
        if (string.IsNullOrEmpty(_options.Name))
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBOptions.Name)} is invalid");
        }
        if (_options.Queues == null || _options.Queues.Count == 0)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBOptions.Queues)} should not be empty.");
        }
    }
}
