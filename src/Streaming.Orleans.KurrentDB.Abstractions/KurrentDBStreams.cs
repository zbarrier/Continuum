namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
///     Well known KurrentDB stream names.
/// </summary>
public static class KurrentDBStreams
{
    /// <summary>
    ///     The <c>$all</c> stream, which holds every event in the database in commit order.
    /// </summary>
    public const string All = "$all";

    /// <summary>
    ///     Returns whether <paramref name="streamName"/> is the <c>$all</c> stream.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    public static bool IsAll(string streamName) => string.Equals(streamName, All, StringComparison.OrdinalIgnoreCase);
}
