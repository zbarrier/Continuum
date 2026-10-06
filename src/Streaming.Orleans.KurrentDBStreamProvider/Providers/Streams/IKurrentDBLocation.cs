namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Location of a message within a KurrentDB.
/// </summary>
public interface IKurrentDBLocation
{
    /// <summary>
    ///     Stream position of the event within a KurrentDB.
    /// </summary>
    string Position { get; }

    /// <summary>
    ///     KurrentDB sequence id of the message
    /// </summary>
    long SequenceNumber { get; }
}
