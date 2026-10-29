using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

/// <summary>
///     Abstraction on KurrentDBProducer class, used to configure KurrentDBProducer class in KurrentDBQueueAdapter,
///     also used to configure KurrentDBGeneratorProducer in KurrentDBQueueAdapter for testing purpose
/// </summary>
public interface IKurrentDBProducer
{
    /// <summary>
    ///     Start to create client and subscribe from KurrentDB persistent subscriptions.
    /// </summary>
    void Init();

    /// <summary>
    ///     Clean up.
    /// </summary>
    Task CloseAsync();

    /// <summary>
    ///     Appends events asynchronously to a stream.
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    Task AppendAsync(params EventData[] events);
}
