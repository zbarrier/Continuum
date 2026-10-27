using System.Diagnostics.CodeAnalysis;

namespace Continuum.Streams;

/// <summary>
/// Decomposes a provider stream name into the identities it encodes.
/// </summary>
/// <remarks>
/// <para>
///     Two concerns meet in a stream name and this interface only owns the first. The composition, which parts a
///     name carries and in what order, belongs to the application. The separators belong to the store, and they
///     are not always ours to choose. KurrentDB derives its <c>$ce-</c> category projections from the first
///     <c>-</c>, so that separator is fixed; CosmosDB names are path shaped and split on <c>/</c>.
/// </para>
/// <para>
///     Implementations are therefore paired to a connection rather than shared, because a single application may
///     read names of different shapes from different stores.
/// </para>
/// </remarks>
public interface IStreamNameParser
{
    /// <summary>
    /// Splits <paramref name="streamName"/> into its category, tenant and key.
    /// </summary>
    IStreamName Parse(string streamName);

    /// <summary>
    /// Attempts to split <paramref name="streamName"/> into its category, tenant and key without throwing when the name
    /// is not in the expected form.
    /// </summary>
    bool TryParse(string streamName, [NotNullWhen(true)] out IStreamName? result);
}
