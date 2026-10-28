using Orleans.Configuration;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Thrown when an event's stream namespace contains <see cref="KurrentDBStreamNamespace.Separator" />, which would
///     make its provider-qualified namespace ambiguous. It is caught by the cache, which skips the event, and never
///     leaves the provider.
/// </summary>
internal sealed class InvalidStreamNamespaceException(string streamNamespace)
    : Exception($"Stream namespace '{streamNamespace}' must not contain '{KurrentDBStreamNamespace.Separator}'.")
{
    public string StreamNamespace { get; } = streamNamespace;
}
