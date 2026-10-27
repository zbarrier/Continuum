namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Thrown when an encoded event does not fit in a single cache block. It is caught by the cache, which skips the
///     event, and never leaves the provider.
/// </summary>
internal sealed class EventTooLargeForCacheException(int size, int blockSize)
    : Exception($"Event of {size} bytes does not fit in a cache block of {blockSize} bytes.")
{
    public int Size { get; } = size;

    public int BlockSize { get; } = blockSize;
}
