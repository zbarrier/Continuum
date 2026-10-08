using Microsoft.Extensions.Logging;

using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public class SubscriberGrain : Grain, ISubscriberGrain
{
    private readonly ILogger<SubscriberGrain> _logger;
    //private readonly ITestOutputHelper _testContext;
    private IAsyncStream<IStreamedEvent<object>> _stream = null!;
    private IStreamProvider _streamProvider = null!;
    private StreamSubscriptionHandle<IStreamedEvent<object>>? _streamSubscription;

    /// <inheritdoc />
    public SubscriberGrain(ILogger<SubscriberGrain> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        //_testContext = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <inheritdoc />
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);
        _streamProvider = this.GetStreamProvider(Constants.StreamProviderName);
    }

    /// <inheritdoc />
    public async Task Subscribe(StreamId streamId)
    {
        _stream = _streamProvider.GetStream<IStreamedEvent<object>>(streamId);
        _streamSubscription = await _stream.SubscribeAsync(HandleNextAsync, HandleExceptionAsync, HandCompleteAsync);
    }

    /// <inheritdoc />
    public async Task Unsubscribe()
    {
        if (_streamSubscription != null)
        {
            await _streamSubscription.UnsubscribeAsync();
            _streamSubscription = null!;
        }
    }

    protected async Task HandleNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken? seq)
    {
        var message = streamedEvent.Event as ChatMessage;
        var formattedMessage = $"#{seq?.SequenceNumber}.{seq?.EventIndex} {message?.Author} said: [{message?.Text}] at {message?.Created:t}";
        _logger.LogInformation(formattedMessage);
        //_testContext.WriteLine(formattedMessage);
    }

    protected async Task HandleExceptionAsync(Exception exception)
    {
        _logger.LogError(exception, exception.Message);
        //_testContext.WriteLine(exception.Message);
    }

    protected async Task HandCompleteAsync()
    {
        _logger.LogInformation($"Stream {Constants.ChatNamespace} is completed.");
        //_testContext.WriteLine($"Stream {Constants.ChatNamespace} is completed.");
    }
}
