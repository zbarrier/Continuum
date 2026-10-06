using System.Globalization;
using Newtonsoft.Json;
using Orleans.Providers.Streams.Common;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     KurrentDB messages consist of a batch of application layer events, so KurrentDB tokens contain three pieces of information.
///     Position - this is a unique value per queue that is used to start reading from this message in the queue.
///     SequenceNumber - KurrentDB sequence numbers are unique ordered message IDs for messages within a queue.
///     The SequenceNumber is required for uniqueness and ordering of KurrentDB messages within a queue.
///     event Index - Since each KurrentDB message may contain more than one application layer event, this value
///     indicates which application layer event this token is for, within a KurrentDB message.  It is required for uniqueness
///     and ordering of application layer events within a KurrentDB message.
/// </summary>
[Serializable]
[GenerateSerializer]
public class KurrentDBSequenceToken : EventSequenceToken, IKurrentDBLocation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBSequenceToken" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor is exposed for serializer use only.
    /// </remarks>
    public KurrentDBSequenceToken()
    {
        Position = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBSequenceToken" /> class.
    /// </summary>
    /// <param name="position">KurrentDB offset within the queue from which this message came.</param>
    /// <param name="sequenceNumber">KurrentDB sequenceNumber for this message.</param>
    /// <param name="eventIndex">Index into a batch of events, if multiple events were delivered within a single KurrentDB message.</param>
    public KurrentDBSequenceToken(string position, long sequenceNumber, int eventIndex)
        : base(sequenceNumber, eventIndex)
    {
        Position = position;
    }

    /// <summary>
    ///     Referring to a potential logical record position in the KurrentDB transaction file.
    /// </summary>
    [JsonProperty]
    [Id(0)]
    public string Position { get; }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    /// <filterpriority>2</filterpriority>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "KurrentDBSequenceToken(Position: {0}, SequenceNumber: {1}, EventIndex: {2})", Position, SequenceNumber, EventIndex);
    }
}
