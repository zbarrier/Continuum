namespace Continuum.Guids;

/// <summary>
///     Creates <see cref="NewId"/> identifiers as sequential RFC 9562 version 8 <see cref="Guid"/> values using
///     <see cref="NewId.NextSequentialGuid"/>.
/// </summary>
/// <remarks>
///     Identifiers sort by a 100 ns timestamp and are strictly ordered per generator. Uses the generator configured on
///     <see cref="NewId"/>. Values are in sequential order, not MSSQL Server <c>uniqueidentifier</c> order.
/// </remarks>
public sealed class NewIdGuidProvider : GuidProvider
{
    /// <inheritdoc />
    public override Guid NewGuid() => NewId.NextSequentialGuid();

    /// <inheritdoc />
    /// <remarks>Reserves all identifiers from the generator in one call.</remarks>
    public override void NewGuids(Span<Guid> destination)
    {
        if (destination.IsEmpty)
            return;

        var ids = NewId.Next(destination.Length);

        for (var i = 0; i < ids.Length; i++)
            destination[i] = ids[i].ToSequentialGuid();
    }

    /// <inheritdoc />
    /// <remarks>The timestamp has 100 ns precision.</remarks>
    public override bool TryGetTimestamp(Guid id, out DateTimeOffset timestamp)
    {
        if (id.Version != 8)
        {
            timestamp = default;
            return false;
        }

        timestamp = NewId.FromSequentialGuid(id).Timestamp;
        return true;
    }
}
