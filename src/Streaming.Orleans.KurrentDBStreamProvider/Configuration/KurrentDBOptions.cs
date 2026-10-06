using Continuum;

using KurrentDB.Client;

namespace Orleans.Configuration;

/// <summary>
///     KurrentDB settings.
/// </summary>
public class KurrentDBOptions
{
    /// <summary>
    ///     The name of the KurrentDB connection used to resolve the keyed <see cref="KurrentDBClient" /> and
    ///     <see cref="KurrentDBPersistentSubscriptionsClient" />, as well as the keyed serializer and type mapper.
    /// </summary>
    [Redact]
    public string ConnectionName { get; set; } = default!;

    /// <summary>
    ///     The KurrentDB client settings.
    /// </summary>
    /// <remarks>
    ///     Optional. When set, the clients registered for <see cref="ConnectionName" /> are built from these settings
    ///     if they have not already been registered by the host.
    /// </remarks>
    [Redact]
    public KurrentDBClientSettings? ClientSettings { get; set; }

    /// <summary>
    ///     The credentials that have permissions to create persistent subscriptions and append/read events.
    ///     <c>UseDefault</c> should be true for insecure localhost connections.
    /// </summary>
    [Redact]
    public KurrentDBStreamCredentialsOptions Credentials { get; set; } = new();

    /// <summary>
    ///     KurrentDB name for this connection, used in cache monitor.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The queue names (aka stream names) of KurrentDB.
    /// </summary>
    public List<string> Queues { get; set; } = new();
}
