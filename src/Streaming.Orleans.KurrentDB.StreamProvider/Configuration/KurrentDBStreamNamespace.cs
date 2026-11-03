namespace Orleans.Configuration;

/// <summary>
///     Builds the provider-qualified Orleans stream namespaces the KurrentDB stream provider delivers on.
/// </summary>
/// <remarks>
///     Orleans matches implicit subscribers by stream namespace only, never by provider name. Several KurrentDB
///     providers reading the same database would otherwise deliver the same event to the same implicit subscriber
///     once per provider. Every delivered stream id therefore has the namespace <c>{providerName}:{namespace}</c>, so an
///     implicit subscription names the provider it belongs to, e.g.
///     <c>[ImplicitStreamSubscription("ProjectionsV2:snack")]</c>. Producers keep writing the bare namespace.
/// </remarks>
public static class KurrentDBStreamNamespace
{
    /// <summary>
    ///     The separator between the provider name and the stream namespace.
    /// </summary>
    public const char Separator = ':';

    /// <summary>
    ///     Returns <c>{providerName}:{streamNamespace}</c>.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     <paramref name="providerName" /> is empty or either argument contains <see cref="Separator" />.
    /// </exception>
    public static string Qualify(string providerName, string streamNamespace)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentNullException.ThrowIfNull(streamNamespace);
        if (providerName.Contains(Separator))
        {
            throw new ArgumentException($"Provider name '{providerName}' must not contain '{Separator}'.", nameof(providerName));
        }
        if (streamNamespace.Contains(Separator))
        {
            throw new ArgumentException($"Stream namespace '{streamNamespace}' must not contain '{Separator}'.", nameof(streamNamespace));
        }
        return $"{providerName}{Separator}{streamNamespace}";
    }
}
