using Orleans.Runtime;

namespace Orleans.Configuration;

/// <summary>
///     Rejects pulling agent settings this provider cannot deliver safely.
/// </summary>
/// <remarks>
///     This provider tracks how far each cursor has read so the <c>$all</c> checkpoint never advances past an event a
///     consumer has not received. That watermark moves when the pulling agent asks the cursor for its next event, which
///     is the only signal the agent gives that it is finished with the current one.
///
///     <see cref="StreamPullingAgentOptions.BatchContainerBatchSize" /> above one breaks that signal: the agent calls
///     <c>MoveNext</c> repeatedly to gather containers before it delivers any of them, so events would be marked as
///     read while still sitting undelivered, and a silo that stopped at that moment would resume past them. Delivery
///     failures become ambiguous too, because one failure report covers a batch the cursor has already advanced
///     through. Failing at start up turns silent event loss after a restart into a configuration error.
/// </remarks>
public class KurrentDBStreamPullingAgentOptionsValidator : IConfigurationValidator
{
    private readonly StreamPullingAgentOptions _options;
    private readonly string _name;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBStreamPullingAgentOptionsValidator" />.
    /// </summary>
    public KurrentDBStreamPullingAgentOptionsValidator(StreamPullingAgentOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (_options.BatchContainerBatchSize > 1)
        {
            throw new OrleansConfigurationException($"{nameof(StreamPullingAgentOptions)} on stream provider {_name} is invalid. {nameof(StreamPullingAgentOptions.BatchContainerBatchSize)} must be 1 for the KurrentDB stream provider, but was {_options.BatchContainerBatchSize}. A larger size lets the pulling agent advance the cursor past events before they are delivered, which would checkpoint the $all position beyond events no consumer received.");
        }
    }
}
