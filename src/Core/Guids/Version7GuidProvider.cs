using System.Buffers.Binary;

namespace Continuum.Guids;

/// <summary>
///     Creates RFC 9562 version 7 identifiers using <see cref="Guid.CreateVersion7(DateTimeOffset)"/>.
/// </summary>
/// <remarks>
///     Identifiers sort by their 48-bit Unix millisecond timestamp. The remaining bits are random, so identifiers created
///     within the same millisecond are not ordered by creation.
/// </remarks>
public sealed class Version7GuidProvider : GuidProvider
{
    readonly TimeProvider _timeProvider;

    /// <summary>Initializes a new instance that uses <see cref="TimeProvider.System"/>.</summary>
    public Version7GuidProvider()
        : this(TimeProvider.System)
    {
    }

    /// <summary>Initializes a new instance that uses the specified time source.</summary>
    /// <param name="timeProvider">The UTC time source for new identifiers.</param>
    public Version7GuidProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public override Guid NewGuid() => Guid.CreateVersion7(_timeProvider.GetUtcNow());

    /// <inheritdoc />
    /// <remarks>The timestamp has millisecond precision.</remarks>
    public override bool TryGetTimestamp(Guid id, out DateTimeOffset timestamp)
    {
        if (id.Version != 7)
        {
            timestamp = default;
            return false;
        }

        Span<byte> bytes = stackalloc byte[16];
        id.TryWriteBytes(bytes, bigEndian: true, out _);

        var milliseconds = (long)(BinaryPrimitives.ReadUInt64BigEndian(bytes) >> 16);

        timestamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        return true;
    }
}
