using Orleans.Configuration;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

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
    ///     Consumer group name, made of the Orleans service id and the stream provider name.
    /// </summary>
    /// <remarks>
    ///     Persistent subscriptions use it as the KurrentDB consumer group. <c>$all</c> subscriptions use it, with the
    ///     connection name, as the checkpoint key.
    /// </remarks>
    public string ConsumerGroup { get; set; } = null!;

    /// <summary>
    ///     The name of a KurrentDB stream.
    /// </summary>
    public string QueueName { get; set; } = null!;

    /// <summary>
    ///     The parser for the names of streams read by the receiver, or <see langword="null" /> for the default.
    /// </summary>
    public Continuum.Streams.IStreamNameParser? StreamNameParser { get; set; }
}
