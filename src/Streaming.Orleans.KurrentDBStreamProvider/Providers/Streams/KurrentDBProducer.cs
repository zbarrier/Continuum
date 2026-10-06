using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Streaming.KurrentDBStorage;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Append events data to KurrentDB stream.
/// </summary>
public class KurrentDBProducer : IKurrentDBProducer
{
    private readonly KurrentProducerSettings _settings;
    private readonly UserCredentials? _credentials;
    private readonly ILogger _logger;

    // NOTE: this client is a keyed singleton owned by the container and shared by every queue of this connection.
    // It must never be disposed by this producer.
    private readonly KurrentDBClient _client;
    private bool _initialized;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBProducer" />.
    /// </summary>
    /// <param name="client">The shared KurrentDB client for this connection.</param>
    /// <param name="settings">The producer settings.</param>
    /// <param name="logger">The logger.</param>
    public KurrentDBProducer(KurrentDBClient client, KurrentProducerSettings settings, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentException.ThrowIfNullOrEmpty(settings.QueueName, nameof(settings.QueueName));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _client = client;
        _settings = settings;
        _credentials = settings.Options.Credentials?.ToUserCredentials();
        _logger = logger;
    }

    /// <summary>
    ///     Marks the producer as ready to append events.
    /// </summary>
    public void Init()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("KurrentDBProducer for stream {QueueName} is initializing.", _settings.QueueName);
        }
        _initialized = true;
    }

    /// <summary>
    ///     Clean up.
    /// </summary>
    public Task CloseAsync()
    {
        // The client is a container owned keyed singleton shared by every queue of this connection, so it is not
        // disposed here.
        _initialized = false;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Appends events asynchronously to a stream.
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    public async Task AppendAsync(params EventData[] events)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Appending events to stream: {QueueName}", _settings.QueueName);
        }
        if (_initialized == false || events == null)
        {
            return;
        }
        try
        {
            await _client.AppendToStreamAsync(_settings.QueueName, StreamState.Any, events, null, null, _credentials).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append events for the the stream {QueueName}.", _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to append events for stream {_settings.QueueName}. {ex.GetType()}: {ex.Message}"));
        }
    }
}
