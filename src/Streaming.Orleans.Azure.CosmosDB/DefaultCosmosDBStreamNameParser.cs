namespace Continuum.Streaming.Orleans.Azure.CosmosDB;

/// <summary>
/// Reads CosmosDB stream names of the form <c>category/key</c>.
/// </summary>
/// <remarks>
/// CosmosDB stream names are path shaped, so the separator is <c>/</c> rather than the <c>-</c> KurrentDB
/// requires. The key is the final segment; anything before it is the category. The topic reported here is the
/// name's own category, which is not the same as the monitored container the change feed reports as the topic.
/// </remarks>
public sealed class DefaultCosmosDBStreamNameParser : IStreamedNameParser
{
    private const char Separator = '/';

    public StreamedName Parse(string streamName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

        var separatorIndex = streamName.LastIndexOf(Separator);
        if (separatorIndex < 0)
        {
            // A name with no separator is all key and has no category of its own to report.
            return new StreamedName(string.Empty, streamName);
        }

        var topic = streamName[..separatorIndex];
        var streamKey = streamName[(separatorIndex + 1)..];

        return new StreamedName(topic, streamKey);
    }
}
