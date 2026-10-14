using Continuum.Streams;
using Continuum.Orleans.Streams;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB;

/// <summary>
/// Reads CosmosDB stream names of the form <c>category/key</c> or <c>category/tenantId|key</c>.
/// </summary>
/// <remarks>
/// CosmosDB stream names are path shaped, so the separator is <c>/</c> rather than the <c>-</c> KurrentDB
/// requires. The key is the final segment; anything before it is the category. A <c>|</c> in the final segment marks
/// the name as tenant qualified. The category reported here is the name's own, which is not the same as the monitored
/// container the change feed reports as the cursor topic.
/// </remarks>
public sealed class DefaultCosmosDBStreamNameParser : IStreamNameParser
{
    private const char Separator = '/';

    /// <summary>Splits the stream name at its last separator into category and key.</summary>
    public IStreamName Parse(string streamName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

        var separatorIndex = streamName.LastIndexOf(Separator);
        if (separatorIndex < 0)
        {
            // A name with no separator is all key and has no category of its own to report.
            return StreamNameParsing.Create(streamName, string.Empty, streamName);
        }

        return StreamNameParsing.Create(streamName, streamName[..separatorIndex], streamName[(separatorIndex + 1)..]);
    }
}
