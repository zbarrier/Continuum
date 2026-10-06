using Orleans.Configuration;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     KurrentDB client settings for stream producer.
/// </summary>
public class KurrentProducerSettings
{
    /// <summary>
    ///     KurrentDB options.
    /// </summary>
    public KurrentDBOptions Options { get; set; } = null!;

    /// <summary>
    ///     The queue name (aks stream name) from KurrentDB.
    /// </summary>
    public string QueueName { get; set; } = null!;
}
