using Microsoft.Extensions.Logging;

using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

/// <inheritdoc cref="ICollectorGrain" />
public class CollectorGrain : Grain, ICollectorGrain
{
    private readonly ILogger<CollectorGrain> _logger;
    private readonly List<ChatMessage> _received = new();
    private StreamSubscriptionHandle<IStreamedEvent<object>>? _subscription;

    /// <inheritdoc />
    public CollectorGrain(ILogger<CollectorGrain> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task Subscribe(StreamId streamId, string? providerName = null)
    {
        var stream = this.GetStreamProvider(providerName ?? Constants.AllStreamProviderName).GetStream<IStreamedEvent<object>>(streamId);
        _subscription = await stream.SubscribeAsync(HandleNextAsync, HandleExceptionAsync);
    }

    /// <inheritdoc />
    public Task<ChatMessage[]> GetReceived()
    {
        return Task.FromResult(_received.ToArray());
    }

    private Task HandleNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken token)
    {
        // The provider delivers the envelope; these tests only assert on the domain event inside it.
        if (streamedEvent.Event is ChatMessage message)
        {
            _received.Add(message);
        }
        return Task.CompletedTask;
    }

    private Task HandleExceptionAsync(Exception exception)
    {
        _logger.LogError(exception, exception.Message);
        return Task.CompletedTask;
    }
}
