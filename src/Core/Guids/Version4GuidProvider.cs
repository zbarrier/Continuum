namespace Continuum.Guids;

/// <summary>
///     Creates random RFC 9562 version 4 identifiers using <see cref="Guid.NewGuid"/>.
/// </summary>
/// <remarks>
///     Version 4 identifiers have no timestamp and no order, so they fragment clustered indexes. Prefer
///     <see cref="NewIdGuidProvider"/> or <see cref="Version7GuidProvider"/> for persisted keys.
/// </remarks>
public sealed class Version4GuidProvider : GuidProvider
{
    /// <inheritdoc />
    public override Guid NewGuid() => Guid.NewGuid();

    /// <summary>Always returns <see langword="false"/>, because version 4 identifiers have no timestamp.</summary>
    /// <param name="id">The identifier to read.</param>
    /// <param name="timestamp">Always <see langword="default"/>.</param>
    /// <returns><see langword="false"/>.</returns>
    public override bool TryGetTimestamp(Guid id, out DateTimeOffset timestamp)
    {
        timestamp = default;
        return false;
    }
}
