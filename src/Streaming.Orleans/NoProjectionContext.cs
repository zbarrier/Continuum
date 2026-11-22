namespace Continuum.Streaming.Orleans;

/// <summary>
/// The context of a projection that needs no dependencies to apply its events.
/// </summary>
public sealed class NoProjectionContext
{
    private NoProjectionContext()
    {
    }

    /// <summary>
    /// The single instance.
    /// </summary>
    public static NoProjectionContext Instance { get; } = new();
}
