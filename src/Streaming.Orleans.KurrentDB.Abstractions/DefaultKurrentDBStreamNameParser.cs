namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
/// Reads KurrentDB stream names of the form <c>category-key</c>.
/// </summary>
/// <remarks>
///     The category is separated from the key by the first <c>-</c>. That separator is not a choice this parser
///     makes: KurrentDB builds its <c>$ce-</c> category projections from the first <c>-</c> in a stream name, so
///     reading the name any other way would disagree with how the store itself groups streams. Everything after
///     that first <c>-</c> is the key, which is what allows a key to contain dashes of its own, as a GUID does.
/// </remarks>
public sealed class DefaultKurrentDBStreamNameParser : IStreamedNameParser
{
    private const char CategorySeparator = '-';

    public StreamedName Parse(string streamName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

        var separatorIndex = streamName.IndexOf(CategorySeparator);
        if (separatorIndex < 0)
        {
            throw new FormatException(
                $"Stream name '{streamName}' has no '{CategorySeparator}' separating its category from its key.");
        }

        var topic = streamName[..separatorIndex];
        var streamKey = streamName[(separatorIndex + 1)..];

        return new StreamedName(topic, streamKey);
    }
}
