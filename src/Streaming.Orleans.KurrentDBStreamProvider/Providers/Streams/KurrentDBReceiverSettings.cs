using Orleans.Configuration;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     KurrentDB subscription settings for stream receiver.
/// </summary>
public class KurrentDBReceiverSettings
{
    /// <summary>
    ///     KurrentDB options.
    /// </summary>
    public KurrentDBOptions Options { get; set; } = null!;

    /// <summary>
    ///     KurrentDB receiver options for the subscription strategy selected on this stream provider.
    ///     Receivers downcast this to the concrete options type they require.
    /// </summary>
    public KurrentDBReceiverOptionsBase ReceiverOptions { get; set; } = null!;

    /// <summary>
    ///     Consumer group name.
    /// </summary>
    public string ConsumerGroup { get; set; } = null!;

    /// <summary>
    ///     The name of a KurrentDB stream.
    /// </summary>
    public string QueueName { get; set; } = null!;
}
