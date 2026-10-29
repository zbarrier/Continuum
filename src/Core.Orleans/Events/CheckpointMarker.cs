using Continuum.TypeMapping;

namespace Continuum.Orleans.Events;

/// <summary>
///     An internal event that marks progress through a subscription without carrying an event of its own.
/// </summary>
/// <remarks>
///     A marker is placed in the same ordered path as real events, so the position it carries only becomes
///     checkpointable once every event before it has been delivered. It belongs to a reserved stream that no
///     application subscribes to.
/// </remarks>
[Alias("Continuum.CheckpointMarker.V1"), GenerateSerializer, Immutable]
[FrameworkType("Continuum.CheckpointMarker.V1")]
public sealed class CheckpointMarker
{
    /// <summary>
    ///     The shared marker instance.
    /// </summary>
    public static readonly CheckpointMarker Instance = new();
}
