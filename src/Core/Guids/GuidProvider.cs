namespace Continuum.Guids;

/// <summary>
///     Creates <see cref="Guid"/> values in one specific format and reads back the timestamp that format embeds, if any.
/// </summary>
/// <remarks>
///     Choose one provider per kind of identifier and keep it. Once identifiers are persisted, changing the provider
///     behind them changes their sort order and index locality, and makes <see cref="TryGetTimestamp"/> unable to read
///     identifiers created by the previous provider.
/// </remarks>
public abstract class GuidProvider
{
    /// <summary>Creates a new <see cref="Guid"/>.</summary>
    /// <returns>A new identifier.</returns>
    public abstract Guid NewGuid();

    /// <summary>Fills <paramref name="destination"/> with new <see cref="Guid"/> values.</summary>
    /// <param name="destination">The span to fill.</param>
    public virtual void NewGuids(Span<Guid> destination)
    {
        for (var i = 0; i < destination.Length; i++)
            destination[i] = NewGuid();
    }

    /// <summary>Reads the creation timestamp embedded in an identifier created by this provider's format.</summary>
    /// <param name="id">The identifier to read.</param>
    /// <param name="timestamp">The UTC creation time, when available.</param>
    /// <returns>
    ///     <see langword="true"/> when <paramref name="id"/> is in this provider's format and the format embeds a
    ///     timestamp; otherwise <see langword="false"/>.
    /// </returns>
    public abstract bool TryGetTimestamp(Guid id, out DateTimeOffset timestamp);
}
