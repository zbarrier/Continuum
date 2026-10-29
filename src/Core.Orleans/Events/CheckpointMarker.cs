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
[Alias(TypeName), GenerateSerializer, Immutable]
[FrameworkType(TypeName)]
public sealed class CheckpointMarker
{
    /// <summary>
    ///     The mapped type name of a marker, used as its event type.
    /// </summary>
    public const string TypeName = "Continuum.CheckpointMarker.V1";

    /// <summary>
    ///     The shared marker instance.
    /// </summary>
    public static readonly CheckpointMarker Instance = new();
}
