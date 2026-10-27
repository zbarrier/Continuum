using System.Diagnostics.CodeAnalysis;

using Continuum.Streams;
using Continuum.Orleans.KurrentDB;
using Continuum.Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
/// Reads KurrentDB stream names of the form <c>category-key</c> or <c>category-tenantId|key</c>.
/// </summary>
/// <remarks>
///     The category is separated from the rest by the first <c>-</c>. That separator is not a choice this parser
///     makes: KurrentDB builds its <c>$ce-</c> category projections from the first <c>-</c> in a stream name, so
///     reading the name any other way would disagree with how the store itself groups streams. Everything after
///     that first <c>-</c> is the key, which is what allows a key to contain dashes of its own, as a GUID does. A
///     <c>|</c> in that remainder marks the name as tenant qualified, with the tenant id before it and the key after.
/// </remarks>
public sealed class DefaultKurrentDBStreamNameParser : IStreamNameParser
{
    private const char CategorySeparator = KurrentDBStreamName.CategorySeparator;

    /// <inheritdoc />
    public IStreamName Parse(string streamName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

        var separatorIndex = streamName.IndexOf(CategorySeparator);
        if (separatorIndex < 0)
        {
            throw new FormatException(
                $"Stream name '{streamName}' has no '{CategorySeparator}' separating its category from its key.");
        }

        return StreamNameParsing.Create(streamName, streamName[..separatorIndex], streamName[(separatorIndex + 1)..]);
    }

    /// <inheritdoc />
    public bool TryParse(string streamName, [NotNullWhen(true)] out IStreamName? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

        var separatorIndex = streamName.IndexOf(CategorySeparator);
        if (separatorIndex >= 0 &&
            StreamNameParsing.TryCreate(streamName, streamName[..separatorIndex], streamName[(separatorIndex + 1)..], out var parsed))
        {
            result = parsed;
            return true;
        }

        result = null;
        return false;
    }
}
