using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     A cursor with no events, handed out when a receiver has no cache because it was shut down or failed to
///     initialize. The pulling agent treats it as a stream that is caught up.
/// </summary>
internal sealed class EmptyCursor : IQueueCacheCursor
{
    public static EmptyCursor Instance { get; } = new();

    private EmptyCursor()
    {
    }

    public IBatchContainer GetCurrent(out Exception? exception)
    {
        exception = null;
        return null!;
    }

    public bool MoveNext() => false;

    public void Refresh(StreamSequenceToken token)
    {
    }

    public void RecordDeliveryFailure()
    {
    }

    public void Dispose()
    {
    }
}
