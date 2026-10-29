using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Appends queued events to a single KurrentDB queue stream. Created by subscription strategies whose stream
///     provider is writable.
/// </summary>
public interface IKurrentDBProducer
{
    /// <summary>
    ///     Marks the producer ready to append.
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
