using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Continuum.Streaming;

public static class IStreamedEventCollectionExtensions
{
    public static Dictionary<string, List<IStreamedEvent<object>>> GroupByStreamName(this List<IStreamedEvent<object>> streamEvents)
    {
        var streamEventsByStreamName = new Dictionary<string, List<IStreamedEvent<object>>>();

        var listAsSpan = CollectionsMarshal.AsSpan(streamEvents);
        ref var searchSpace = ref MemoryMarshal.GetReference(listAsSpan);
        for (int i = 0; i < listAsSpan.Length; i++)
        {
            var streamEvent = Unsafe.Add(ref searchSpace, i);
            ref var valueOrNew = ref CollectionsMarshal.GetValueRefOrAddDefault(streamEventsByStreamName, streamEvent.StreamName, out bool exists);
            if (exists)
            {
                valueOrNew!.Add(streamEvent);
            }
            else
            {
                valueOrNew = [streamEvent];
            }
        }

        return streamEventsByStreamName;
    }

    public static Dictionary<string, List<IStreamedEvent<object>>> GroupByStreamKey(this List<IStreamedEvent<object>> streamEvents)
    {
        var streamEventsByStreamKey = new Dictionary<string, List<IStreamedEvent<object>>>();

        var listAsSpan = CollectionsMarshal.AsSpan(streamEvents);
        ref var searchSpace = ref MemoryMarshal.GetReference(listAsSpan);
        for (int i = 0; i < listAsSpan.Length; i++)
        {
            var streamEvent = Unsafe.Add(ref searchSpace, i);
            ref var valueOrNew = ref CollectionsMarshal.GetValueRefOrAddDefault(streamEventsByStreamKey, streamEvent.StreamKey, out bool exists);
            if (exists)
            {
                valueOrNew!.Add(streamEvent);
            }
            else
            {
                valueOrNew = [streamEvent];
            }
        }

        return streamEventsByStreamKey;
    }
}
